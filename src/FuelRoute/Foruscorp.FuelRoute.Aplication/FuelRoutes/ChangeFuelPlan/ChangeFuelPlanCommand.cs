using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Domain.RouteValidators;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.POIFS.Macros;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.ChangeFuelPlan
{
    public enum Operation
    {
        Add = 1,
        Remove = 2,
        Update = 3
    }

    public record ChangeFuelPlanCommand : IRequest<Result<ChangeFuelPlanResponse>>
    {
        public Guid RouteSectionId { get; init; }
        public double CurrentFuelPercent { get; init; }
        public FuelStationChangeDto FuelStationChange { get; init; }
        public Operation Operation { get; init; }
    }

    public record FuelStationChangeDto
    {
        public Guid FuelStationId { get; init; }
        public double? NewRefill { get; init; }
    }

    public record ChangeFuelPlanResponse
    {
        //public Guid RouteValidatorId { get; init; }
        public bool IsValid { get; init; }
        public List<FuelStationChangeInfo> Changes { get; init; } = [];
        //public string Message { get; init; } = string.Empty;
        //public string ValidationDetails { get; init; } = string.Empty;
        //public List<string> ValidationWarnings { get; init; } = [];
        //public double FinalFuelAmount { get; init; }
        public List<ValidationStepResult> StepResults { get; set; } = [];
    }

    public record FuelStationChangeInfo
    {
        public Guid FuelStationId { get; init; }
        public double OriginalRefill { get; init; }
        public double NewRefill { get; init; }
        public double OriginalCurrentFuel { get; init; }
        public double NewCurrentFuel { get; init; }
        public bool IsAlgo { get; init; }
        public bool IsManual { get; init; }
        public string Status { get; init; } = string.Empty;
    }

    public class ChangeFuelPlanCommandHandler(
        IFuelRouteContext fuelRouteContext,
        ILogger<ChangeFuelPlanCommandHandler> logger) : IRequestHandler<ChangeFuelPlanCommand, Result<ChangeFuelPlanResponse>>
    {
        public async Task<Result<ChangeFuelPlanResponse>> Handle(ChangeFuelPlanCommand request, CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation("Starting fuel plan change for route section {RouteSectionId}", request.RouteSectionId);

                // Получаем секцию маршрута с топливными станциями
                var routeSection = await fuelRouteContext.RouteSections
                    .Include(rs => rs.FuelRouteStations)
                    .Include(rs => rs.FuelRoute)
                    .FirstOrDefaultAsync(rs => rs.Id == request.RouteSectionId, cancellationToken);

                if (routeSection == null)
                {
                    logger.LogWarning("Route section {RouteSectionId} not found", request.RouteSectionId);
                    return Result.Fail<ChangeFuelPlanResponse>($"Route section {request.RouteSectionId} not found");
                }


                var routeValidator = await fuelRouteContext.RouteValidators
                    .Include(rv => rv.FuelStationChanges)
                    .FirstOrDefaultAsync(rv => rv.FuelRouteSectionId == routeSection.Id, cancellationToken);


                if (routeValidator == null)
                {
                    await CalculateAndSetForwardDistances(routeSection, logger);
                    routeValidator = new RouteValidator(routeSection.FuelRoute, routeSection);
                    fuelRouteContext.RouteValidators.Add(routeValidator);
                }

                var changes = new List<FuelStationChangeInfo>();

                var changeDto = request.FuelStationChange;
                var fuelStation = routeSection.FuelRouteStations
                    .FirstOrDefault(fs => fs.FuelPointId == changeDto.FuelStationId);

                if (fuelStation == null)
                {
                    logger.LogWarning("Fuel station {FuelStationId} not found in route section {RouteSectionId}", 
                        changeDto.FuelStationId, request.RouteSectionId);
                    return Result.Fail<ChangeFuelPlanResponse>($"Fuel station {changeDto.FuelStationId} not found");
                }

                var originalRefill = double.TryParse(fuelStation.Refill, out var refill) ? refill : 0.0;
                var originalCurrentFuel = fuelStation.CurrentFuel;

                switch (request.Operation)
                {
                    case Operation.Add:
                        // Добавляем новую топливную станцию

                        var fuelStationChange = routeValidator.FuelStationChanges.FirstOrDefault(x => x.FuelStation.FuelPointId == fuelStation.FuelPointId);

                        if(fuelStationChange != null) goto case Operation.Update;

                        fuelStationChange = FuelStationChange.CreateManual(fuelStation);

                        if (changeDto.NewRefill.HasValue)
                            fuelStationChange.Refill = changeDto.NewRefill.Value;


                        routeValidator.AddFuelStationChange(fuelStationChange);

                        changes.Add(new FuelStationChangeInfo
                        {
                            FuelStationId = changeDto.FuelStationId,
                            OriginalRefill = 0.0,
                            NewRefill = fuelStationChange.Refill,
                            OriginalCurrentFuel = 0.0,
                            NewCurrentFuel = fuelStationChange.CurrentFuel,
                            IsAlgo = fuelStationChange.IsAlgo,
                            IsManual = fuelStationChange.IsManual,
                            Status = "Added"
                        });
                        break;

                    case Operation.Remove:
                        // Удаляем топливную станцию
                        var existingChange = routeValidator.FuelStationChanges
                            .FirstOrDefault(fsc => fsc.FuelRouteStationId == fuelStation.FuelStationId);
                        
                        if (existingChange != null)
                        {
                            routeValidator.RemoveFuelStationChange(existingChange.FuelRouteStationId);
                        }

                        changes.Add(new FuelStationChangeInfo
                        {
                            FuelStationId = changeDto.FuelStationId,
                            OriginalRefill = originalRefill,
                            NewRefill = 0.0,
                            OriginalCurrentFuel = originalCurrentFuel,
                            NewCurrentFuel = 0.0,
                            IsAlgo = false,
                            IsManual = false,
                            Status = "Removed"
                        });
                        break;

                    case Operation.Update:
                        // Обновляем существующую топливную станцию


                       var change = routeValidator.FuelStationChanges.FirstOrDefault(x => x.FuelStation.FuelPointId == fuelStation.FuelPointId);

                        if (change == null) goto case Operation.Add;

                       change.IsManual = true;
                        //var updateFuelStationChange = changeDto.IsManual 
                        //    ? FuelStationChange.CreateManual(fuelStation, changeDto.ForwardDistance ?? 0)
                        //    : FuelStationChange.CreateAlgo(fuelStation);

                        if (changeDto.NewRefill.HasValue)
                            change.Refill = changeDto.NewRefill.Value;
                        //if (changeDto.NewCurrentFuel.HasValue)
                        //    change.CurrentFuel = changeDto.NewCurrentFuel.Value;

                        

                        //routeValidator.AddFuelStationChange(updateFuelStationChange);

                        changes.Add(new FuelStationChangeInfo
                        {
                            FuelStationId = changeDto.FuelStationId,
                            OriginalRefill = originalRefill,
                            NewRefill = change.Refill,
                            OriginalCurrentFuel = originalCurrentFuel,
                            NewCurrentFuel = change.CurrentFuel,
                            IsAlgo = change.IsAlgo,
                            IsManual = change.IsManual,
                            Status = "Updated"
                        });
                        break;

                    default:
                        return Result.Fail<ChangeFuelPlanResponse>($"Unknown operation: {request.Operation}");
                }

   
                var fuelPlanValidator = new FuelPlanValidator();
                var validationResult = fuelPlanValidator.ValidatePlanDetailed(routeSection, routeValidator.FuelStationChanges, routeSection.FuelRoute.Weight, request.CurrentFuelPercent, 200.0, 400.0, 0.18, routeSection.FuelRoute.RemainingFuel);
                routeValidator.IsValid = validationResult.IsValid;

                // Логируем детали валидации
                if (!validationResult.IsValid)
                {
                    logger.LogWarning("Fuel plan validation failed: {FailureReason}", validationResult.FailureReason);
                }

                foreach (var warning in validationResult.Warnings)
                {
                    logger.LogWarning("Fuel plan validation warning: {Warning}", warning);
                }

                routeValidator.FinalFuelAmount = validationResult.FinalFuelAmount;

                // Сохраняем изменения в базу данных
                await fuelRouteContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Fuel plan {Operation} completed for route section {RouteSectionId}. Valid: {IsValid}", 
                    request.Operation, request.RouteSectionId, validationResult.IsValid);

                var response = new ChangeFuelPlanResponse
                {
                    IsValid = validationResult.IsValid,
                    Changes = changes,
                    //ValidationDetails = validationResult.FailureReason,
                    //ValidationWarnings = validationResult.Warnings,
                    //FinalFuelAmount = validationResult.FinalFuelAmount,
                    StepResults = validationResult.StepResults.Where(x => x.IsValid == false).ToList()
                };

                return Result.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error changing fuel plan for route section {RouteSectionId}", request.RouteSectionId);
                return Result.Fail<ChangeFuelPlanResponse>($"Error changing fuel plan: {ex.Message}");
            }
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
            const double searchRadiusKm = 5.0;

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
