using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.ChangeFuelPlan;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Domain.RouteValidators;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.FuelRoutes.Aplication.FuelPlanns.ValidateRoutePlann
{
    public record ValidateResponse
    {
        public bool IsValid { get; init; }

        public List<ValidationStepResult> StepResults { get; set; } = [];
    }
    public record ValidateRoutePlannCommand(Guid RouteSectionId, double CurrentFuelPercentage) : IRequest<Result<ValidateResponse>>;
    public class ValidateRoutePlannCommandHandler(
        IFuelRouteContext fuelRouteContext,
        ILogger<ValidateRoutePlannCommandHandler> logger) : IRequestHandler<ValidateRoutePlannCommand, Result<ValidateResponse>>
    {
        public async Task<Result<ValidateResponse>> Handle(ValidateRoutePlannCommand request, CancellationToken cancellationToken)
        {
            var routeSection = await fuelRouteContext.RouteSections
                .Include(rs => rs.FuelRouteStations)
                .Include(rs => rs.RouteValidator)
                  .ThenInclude(rv => rv.FuelStationChanges)
                .Include(rs => rs.FuelRoute)
                .AsSplitQuery()    
                .FirstOrDefaultAsync(rs => rs.Id == request.RouteSectionId);

            if (routeSection == null)
            {
                logger.LogWarning("Route section with ID {RouteSectionId} not found.", request.RouteSectionId);
                return Result.Fail("Route section not found.");
            }

            var routeValidator = routeSection.RouteValidator;

            if (routeSection.RouteValidator == null)
            {
                await CalculateAndSetForwardDistances(routeSection, logger);
                routeValidator = new RouteValidator(routeSection.FuelRoute, routeSection);
                fuelRouteContext.RouteValidators.Add(routeValidator);
                //await fuelRouteContext.SaveChangesAsync(cancellationToken);
            }

            var fuelPlanValidator = new FuelPlanValidator();
            var validationResult = fuelPlanValidator.ValidatePlanDetailed(routeSection, routeValidator.FuelStationChanges, routeSection.FuelRoute.Weight, request.CurrentFuelPercentage, 200.0, 400.0, 0.18, routeSection.FuelRoute.RemainingFuel - 2);
            routeValidator.IsValid = validationResult.IsValid;
            routeValidator.FinalFuelAmount = validationResult.FinalFuelAmount;  

            await fuelRouteContext.SaveChangesAsync(cancellationToken);

            var result = new ValidateResponse
            {
                IsValid = validationResult.IsValid,
                StepResults = validationResult.StepResults.Where(x => x.IsValid == false).ToList()
            };

            return result;
        }

        private async Task CalculateAndSetForwardDistances(FuelRouteSection routeSection, ILogger logger)
        {
            try
            {
                logger.LogInformation("Starting automatic ForwardDistance calculation for route section {RouteSectionId}", routeSection.Id);

                var stations = routeSection.FuelRouteStations
                    .Where(fs => !string.IsNullOrEmpty(fs.Latitude) && !string.IsNullOrEmpty(fs.Longitude))
                    .OrderBy(fs => fs.StopOrder)
                    .ToList();

                if (!stations.Any())
                {
                    logger.LogWarning("No valid fuel stations found for ForwardDistance calculation");
                    return;
                }

                // Получаем точки маршрута из полилайна
                var routePoints = ExtractRoutePointsFromPolyline(routeSection.EncodeRoute);

                if (!routePoints.Any())
                {
                    logger.LogWarning("No route points found in polyline for ForwardDistance calculation");
                    return;
                }

                var updatedStations = new List<FuelRouteStation>();
                var totalStations = stations.Count;

                for (int i = 0; i < totalStations; i++)
                {
                    var currentStation = stations[i];
                    var forwardDistance = 0.0;

                    // Используем рекурсивную логику для расчета ForwardDistance
                    if (double.TryParse(currentStation.Latitude, out var lat) && double.TryParse(currentStation.Longitude, out var lon))
                    {
                        var stationCoords = new GeoPoint(lat, lon);
                        forwardDistance = CalculateForwardDistanceRecursively(routePoints, stationCoords, 0, 0.0);

                        logger.LogDebug("Calculated ForwardDistance for station {StationId}: {Distance:F1}km using recursive algorithm",
                            currentStation.FuelStationId, forwardDistance);
                    }

                    // Устанавливаем ForwardDistance только если он еще не установлен или равен 0
                    if (currentStation.ForwardDistance == 0.0 && forwardDistance > 0 && forwardDistance < double.MaxValue)
                    {
                        currentStation.SetForwardDistance(forwardDistance);
                        updatedStations.Add(currentStation);

                        logger.LogInformation("Set ForwardDistance for station {StationId}: {Distance:F1}km",
                            currentStation.FuelStationId, forwardDistance);
                    }
                    else if (currentStation.ForwardDistance > 0)
                    {
                        logger.LogDebug("ForwardDistance already set for station {StationId}: {Distance:F1}km",
                            currentStation.FuelStationId, currentStation.ForwardDistance);
                    }
                    else
                    {
                        logger.LogWarning("Could not calculate valid ForwardDistance for station {StationId}",
                            currentStation.FuelStationId);
                    }
                }

                // Сохраняем изменения в базу данных
                if (updatedStations.Any())
                {
                    fuelRouteContext.FuelRouteStation.UpdateRange(updatedStations);
                    await fuelRouteContext.SaveChangesAsync();

                    logger.LogInformation("Successfully updated ForwardDistance for {Count} stations in route section {RouteSectionId}",
                        updatedStations.Count, routeSection.Id);
                }
                else
                {
                    logger.LogInformation("No ForwardDistance updates needed for route section {RouteSectionId}", routeSection.Id);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error calculating ForwardDistance for route section {RouteSectionId}", routeSection.Id);
                throw;
            }
        }


        private double CalculateForwardDistanceRecursively(
            List<GeoPoint> route,
            GeoPoint stationCoords,
            int segmentIndex,
            double cumulativeDistance)
        {
            // Базовый случай: достигли конца маршрута
            if (segmentIndex >= route.Count - 1)
                return double.MaxValue;

            var segmentStart = route[segmentIndex];
            var segmentEnd = route[segmentIndex + 1];
            var segmentLength = CalculateHaversineDistance(segmentStart, segmentEnd);
            var distanceToSegment = DistanceFromPointToSegment(stationCoords, segmentStart, segmentEnd);

            // Радиус поиска станций (в км)
            const double searchRadiusKm = 10.0;

            // Если станция достаточно близко к текущему сегменту
            if (distanceToSegment <= searchRadiusKm)
            {
                var projectionDistance = DistanceAlongSegment(segmentStart, segmentEnd, stationCoords);
                return cumulativeDistance + projectionDistance;
            }

            // Рекурсивный вызов для следующего сегмента
            return CalculateForwardDistanceRecursively(
                route,
                stationCoords,
                segmentIndex + 1,
                cumulativeDistance + segmentLength);
        }

        /// <summary>
        /// Извлекает точки маршрута из полилайна
        /// </summary>
        private List<GeoPoint> ExtractRoutePointsFromPolyline(string encodedPolyline)
        {
            try
            {
                var polylinePoints = PolylineEncoder.DecodePolyline(encodedPolyline);
                return polylinePoints.Select(p => new GeoPoint(p[0], p[1])).ToList();
            }
            catch
            {
                return new List<GeoPoint>();
            }
        }

        /// <summary>
        /// Рассчитывает расстояние по формуле Haversine между двумя точками
        /// </summary>
        private double CalculateHaversineDistance(GeoPoint point1, GeoPoint point2)
        {
            return FuelRouteStation.CalculateDistanceBetweenPoints(point1.Latitude, point1.Longitude, point2.Latitude, point2.Longitude);
        }

        /// <summary>
        /// Рассчитывает расстояние от точки до сегмента маршрута
        /// </summary>
        private double DistanceFromPointToSegment(GeoPoint point, GeoPoint segmentStart, GeoPoint segmentEnd)
        {
            // Простая реализация - расстояние до ближайшей точки сегмента
            var distanceToStart = CalculateHaversineDistance(point, segmentStart);
            var distanceToEnd = CalculateHaversineDistance(point, segmentEnd);

            return Math.Min(distanceToStart, distanceToEnd);
        }

        /// <summary>
        /// Рассчитывает расстояние вдоль сегмента до проекции точки
        /// </summary>
        private double DistanceAlongSegment(GeoPoint segmentStart, GeoPoint segmentEnd, GeoPoint point)
        {
            // Простая реализация - возвращаем половину длины сегмента
            // В реальной реализации здесь должна быть более сложная геометрия
            return CalculateHaversineDistance(segmentStart, segmentEnd) / 2.0;
        }

    }


}
