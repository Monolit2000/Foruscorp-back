using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class PlanFuelStationsByRoadsResponce
    {
        public List<FuelStationDto> FuelStations { get; set; } = new List<FuelStationDto>();
        public FinishInfo FinishInfo { get; set; } = new FinishInfo();
    }

    public class GetFuelStationsByRoadsQueryHandler :
        IRequestHandler<GetFuelStationsByRoadsQuery, Result<PlanFuelStationsByRoadsResponce>>
    {
        private readonly IFuelStationContext _fuelStationContext;
        private readonly ITruckProviderService _truckProviderService;
        private readonly FuelStopStationPlanner _fuelStopStationPlanner;
        private readonly FuelStationMapper _fuelStationMapper;
        private readonly RouteValidator _routeValidator;
        private readonly FuelConsumptionCalculator _fuelConsumptionCalculator;

        public GetFuelStationsByRoadsQueryHandler(
            IFuelStationContext fuelStationContext, 
            ITruckProviderService truckProviderService)
        {
            _fuelStationContext = fuelStationContext;
            _truckProviderService = truckProviderService;
            _fuelStopStationPlanner = new FuelStopStationPlanner();
            _fuelStationMapper = new FuelStationMapper();
            _routeValidator = new RouteValidator();
            _fuelConsumptionCalculator = new FuelConsumptionCalculator();
        }

        public async Task<Result<PlanFuelStationsByRoadsResponce>> Handle(
            GetFuelStationsByRoadsQuery request,
            CancellationToken cancellationToken)
        {
            var validationResult = _routeValidator.ValidateRequest(request);
            if (validationResult.IsFailed)
                return Result.Fail(validationResult.Errors);

            var fuelParameters = _fuelConsumptionCalculator.CalculateFuelParameters(request);

            var routePoints = _routeValidator.ExtractRoutePoints(request.Roads);
            
            var stations = await GetStationsInBoundingBox(routePoints, cancellationToken);
            if (!stations.Any())
                return Result.Ok(new PlanFuelStationsByRoadsResponce());

            var routeResults = await PlanStopsForAllRoads(
                request.Roads, 
                stations, 
                request, 
                fuelParameters);

            var result = CombineRouteResults(routeResults);

            return Result.Ok(result);
        }

        private async Task<List<FuelStation>> GetStationsInBoundingBox(
            List<GeoPoint> routePoints, 
            CancellationToken cancellationToken)
        {
            var boundingBox = _routeValidator.CalculateBoundingBox(routePoints);
            
            return await _fuelStationContext.FuelStations
                .Include(s => s.FuelPrices)
                .AsNoTracking()
                .Where(s =>
                    s.Coordinates.Latitude >= boundingBox.MinLat &&
                    s.Coordinates.Latitude <= boundingBox.MaxLat &&
                    s.Coordinates.Longitude >= boundingBox.MinLon &&
                    s.Coordinates.Longitude <= boundingBox.MaxLon)
                .ToListAsync(cancellationToken);
        }

        private async Task<List<RouteStopsForRoadInfo>> PlanStopsForAllRoads(
            List<RoadSectionDto> roads,
            List<FuelStation> stations,
            GetFuelStationsByRoadsQuery request,
            FuelParameters fuelParameters)
        {
            var tasks = roads.Select(road =>
                Task.Run(() => PlanRouteStopsForRoad(
                    road,
                    stations,
                    request.RequiredFuelStations,
                    request.FuelProviderNameList,
                    fuelParameters,
                    request.FinishFuel)));

            return (await Task.WhenAll(tasks)).ToList();
        }

        private RouteStopsForRoadInfo PlanRouteStopsForRoad(
            RoadSectionDto road,
            List<FuelStation> allStations,
            List<RequiredStationDto> requiredStationDtos,
            List<string> fuelProviderNameList,
            FuelParameters fuelParameters,
            double finishFuel = 40)
        {
            var routePoints = _routeValidator.ExtractRoutePoints(new List<RoadSectionDto> { road });
            if (routePoints.Count < 2)
                return new RouteStopsForRoadInfo();

            var providerFilter = CreateProviderFilter(fuelProviderNameList);
            var stationsAlongRoute = FilterStationsAlongRoute(allStations, routePoints, providerFilter);
            
            if (!stationsAlongRoute.Any())
                return new RouteStopsForRoadInfo();

            var fuelstationWithoutAlgorithm = stationsAlongRoute
                .Select(x => _fuelStationMapper.ToDtoNoAlgorithm(x, road.RoadSectionId))
                .ToList();

            var totalRouteDistanceKm = CalculateTotalRouteDistance(routePoints);

            //var newSDSD = new RefactoredFuelStopStationPlanner();

            //var stopPlan = newSDSD.PlanStopsWithComprehensiveOptimization(
            //    routePoints,
            //    stationsAlongRoute,
            //    totalRouteDistanceKm,
            //    fuelParameters.ConsumptionGPerKm,
            //    fuelParameters.InitialFuelPercent,
            //    fuelParameters.TankCapacityG,
            //    requiredStationDtos,
            //    finishFuel,
            //    road.RoadSectionId);

            var dijkstra = new DijkstraFuelOptimizer();

            var stopPlan = dijkstra.FindOptimalRoute(
                routePoints,
                stationsAlongRoute,
                totalRouteDistanceKm,
                fuelParameters.ConsumptionGPerKm,
                fuelParameters.InitialFuelPercent,
                fuelParameters.TankCapacityG,
                requiredStationDtos,
                finishFuel);





            return new RouteStopsForRoadInfo
            {
                StopPlan = stopPlan.StopPlan,
                StationsWithoutAlgorithm = fuelstationWithoutAlgorithm,
                Finish = stopPlan.Finish
            };
        }

        private List<string> CreateProviderFilter(List<string> fuelProviderNameList)
        {
            return (fuelProviderNameList ?? new List<string>())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.ToLowerInvariant())
                .ToList();
        }

        private List<FuelStation> FilterStationsAlongRoute(
            List<FuelStation> allStations, 
            List<GeoPoint> routePoints, 
            List<string> providerFilter)
        {
            return allStations
                .Where(s => routePoints.Any(geoPoint => 
                    GeoCalculator.IsPointWithinRadius(geoPoint, s.Coordinates, RouteValidator.SearchRadiusKm)))
                .DistinctBy(s => s.Id)
                .Where(s => !providerFilter.Any() || 
                           providerFilter.Contains(s.ProviderName?.ToLowerInvariant() ?? string.Empty))
                .ToList();
        }

        private double CalculateTotalRouteDistance(List<GeoPoint> routePoints)
        {
            double totalDistance = 0;
            for (int i = 0; i < routePoints.Count - 1; i++)
            {
                totalDistance += GeoCalculator.CalculateHaversineDistance(routePoints[i], routePoints[i + 1]);
            }
            return totalDistance;
        }

        private PlanFuelStationsByRoadsResponce CombineRouteResults(List<RouteStopsForRoadInfo> routeResults)
        {
            var allStopPlans = routeResults.SelectMany(r => r.StopPlan).ToList();
            var allStationsWithoutAlgo = routeResults.SelectMany(r => r.StationsWithoutAlgorithm).ToList();
            var finishInfo = routeResults.FirstOrDefault()?.Finish ?? new FinishInfo();

            if (!allStopPlans.Any())
            {
                return new PlanFuelStationsByRoadsResponce 
                { 
                    FuelStations = _fuelStationMapper.RemoveDuplicatesByCoordinates(allStationsWithoutAlgo) 
                };
            }

            var resultDto = _fuelStationMapper.MapStopPlansToDtos(allStopPlans);
            var zipStations = _fuelStationMapper.ZipStations(allStationsWithoutAlgo, resultDto);

            return new PlanFuelStationsByRoadsResponce
            {
                FuelStations = zipStations,
                FinishInfo = finishInfo
            };
        }
    }

    public class RouteValidator
    {
        public const double SearchRadiusKm = 9.0;

        public Result ValidateRequest(GetFuelStationsByRoadsQuery request)
        {
            if (request?.Roads == null || !request.Roads.Any(r => r.Points?.Any(p => p?.Count >= 2) == true))
                return Result.Fail("No valid roads or points provided.");

            return Result.Ok();
        }

        public List<GeoPoint> ExtractRoutePoints(List<RoadSectionDto> roads)
        {
            var routePoints = roads
                .SelectMany(r => r.Points.Where(p => p?.Count >= 2)
                    .Select(p => new GeoPoint(p[0], p[1])))
                .ToList();

            return routePoints;
        }

        public BoundingBox CalculateBoundingBox(List<GeoPoint> routePoints)
        {
            double avgLatRadians = DegreesToRadians(routePoints.Average(pt => pt.Latitude));
            
            return new BoundingBox
            {
                MinLat = routePoints.Min(pt => pt.Latitude) - (SearchRadiusKm / 111.0),
                MaxLat = routePoints.Max(pt => pt.Latitude) + (SearchRadiusKm / 111.0),
                MinLon = routePoints.Min(pt => pt.Longitude) - (SearchRadiusKm / (111.0 * Math.Cos(avgLatRadians))),
                MaxLon = routePoints.Max(pt => pt.Longitude) + (SearchRadiusKm / (111.0 * Math.Cos(avgLatRadians)))
            };
        }

        private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180.0);
    }

    public class FuelConsumptionCalculator
    {
        private const double ReferenceWeightLb = 40000.0;
        private const double ReferenceConsumptionGPerKm = 0.089;

        public FuelParameters CalculateFuelParameters(GetFuelStationsByRoadsQuery request)
        {
            var tankCapacityG = 200.0;
            var initialFuelPercent = tankCapacityG * (request.CurrentFuel / 100.0);

            return new FuelParameters
            {
                ConsumptionGPerKm = CalculateConsumptionGPerKm(request.Weight),
                TankCapacityG = tankCapacityG,
                InitialFuelPercent = initialFuelPercent
            };
        }

        private static double CalculateConsumptionGPerKm(double weightLb)
        {
            return ReferenceConsumptionGPerKm * (weightLb / ReferenceWeightLb);
        }
    }

    public class FuelStationMapper
    {
        public List<FuelStationDto> MapStopPlansToDtos(List<FuelStopPlan> allStopPlans)
        {
            var result = new List<FuelStationDto>();

            var groupedBySection = allStopPlans.GroupBy(x => x.RoadSectionId).ToList();

            foreach (var group in groupedBySection)
            {
                var sectionStops = group.OrderBy(x => x.StopAtKm).ToList();

                for (int i = 0; i < sectionStops.Count; i++)
                {
                    var current = sectionStops[i];
                    double nextDistanceKm = i < sectionStops.Count - 1 
                        ? sectionStops[i + 1].StopAtKm - current.StopAtKm 
                        : 0;

                    result.Add(ToDto(
                        current.Station,
                        stopOrder: i + 1,
                        refillLiters: current.RefillLiters,
                        nextDistanceKm: nextDistanceKm,
                        roadSectionId: current.RoadSectionId,
                        currentFuel: current.CurrentFuelLiters));
                }
            }

            return result;
        }

        public List<FuelStationDto> RemoveDuplicatesByCoordinates(List<FuelStationDto> fuelStations)
        {
            return fuelStations
                .GroupBy(station => (station.Latitude, station.Longitude))
                .Select(group => group.OrderByDescending(station => station.IsAlgorithm).First())
                .ToList();
        }

        public List<FuelStationDto> ZipStations(List<FuelStationDto> fuelstationWithoutAlgorithm, List<FuelStationDto> stopPlan)
        {
            return fuelstationWithoutAlgorithm.Select(station =>
            {
                var matchingStation = stopPlan.FirstOrDefault(s => 
                    s.Id == station.Id && s.RoadSectionId == station.RoadSectionId);
                
                if (matchingStation != null)
                {
                    return CreateFuelStationDto(station, matchingStation);
                }
                
                return CreateFuelStationDto(station, station);
            }).ToList();
        }

        public FuelStationDto ToDto(
            FuelStation station,
            int stopOrder,
            double refillLiters,
            double nextDistanceKm,
            string roadSectionId,
            double currentFuel)
        {
            var priceInfo = station?.FuelPrices.FirstOrDefault();
            string pricePerLiter = priceInfo?.Price.ToString("F2") ?? "0.00";
            
            return new FuelStationDto
            {
                Id = station!.Id,
                Name = station.ProviderName,
                Address = station.Address,
                Latitude = station.Coordinates.Latitude.ToString("F6"),
                Longitude = station.Coordinates.Longitude.ToString("F6"),
                Price = pricePerLiter,
                Discount = priceInfo?.DiscountedPrice?.ToString("F2"),
                PriceAfterDiscount = priceInfo?.PriceAfterDiscount.ToString("F2"),
                IsAlgorithm = true,
                Refill = refillLiters.ToString("F2"),
                StopOrder = stopOrder,
                NextDistanceKm = nextDistanceKm.ToString("F2"),
                RoadSectionId = roadSectionId,
                CurrentFuel = currentFuel
            };
        }

        public FuelStationDto ToDtoNoAlgorithm(FuelStation station, string roadSectionId)
        {
            var priceInfo = station?.FuelPrices.FirstOrDefault();
            string pricePerLiter = priceInfo?.Price.ToString("F2") ?? "0.00";
            
            return new FuelStationDto
            {
                Id = station!.Id,
                Name = station.ProviderName,
                Address = station.Address,
                Latitude = station.Coordinates.Latitude.ToString("F6"),
                Longitude = station.Coordinates.Longitude.ToString("F6"),
                Price = pricePerLiter,
                Discount = priceInfo?.DiscountedPrice?.ToString("F2"),
                PriceAfterDiscount = priceInfo?.PriceAfterDiscount.ToString("F2"),
                IsAlgorithm = false,
                RoadSectionId = roadSectionId
            };
        }

        private FuelStationDto CreateFuelStationDto(FuelStationDto baseStation, FuelStationDto algorithmStation)
        {
            return new FuelStationDto
            {
                Id = baseStation.Id,
                Name = baseStation.Name,
                Address = baseStation.Address,
                Latitude = baseStation.Latitude,
                Longitude = baseStation.Longitude,
                Price = baseStation.Price,
                Discount = baseStation.Discount,
                PriceAfterDiscount = baseStation.PriceAfterDiscount,
                IsAlgorithm = algorithmStation.IsAlgorithm,
                Refill = algorithmStation.Refill,
                StopOrder = algorithmStation.StopOrder,
                NextDistanceKm = algorithmStation.NextDistanceKm,
                RoadSectionId = algorithmStation.RoadSectionId,
                CurrentFuel = algorithmStation.CurrentFuel
            };
        }
    }

    public class FuelParameters
    {
        public double ConsumptionGPerKm { get; set; }
        public double TankCapacityG { get; set; }
        public double InitialFuelPercent { get; set; }
    }

    public class BoundingBox
    {
        public double MinLat { get; set; }
        public double MaxLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLon { get; set; }
    }

    public class TruckMapping
    {
        public Guid TruckId { get; set; }
        public string ProviderTruckId { get; set; }
    }
}
