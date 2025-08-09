using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;
using System.Collections.Generic;
using static Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.GetFuelStationsByRoadsQueryHandler;

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
        // Радиус «коридора» вдоль маршрута (в км), в пределах которого принимаем станции
        private const double SearchRadiusKm = 9.0;
        private static double TankRestrictions = 40.0;

        
        //// Расход топлива: 0.3 л/км (30 л/100 км)
        //private const double TruckFuelConsumptionGPerKm = 0.3;

        //// Ёмкость бака в галонах
        //private const double TruckTankCapacityG = 200.0;

        //// Начальный объём топлива в галонах (можно брать из запроса, здесь для примера)
        //private const double InitialFuelPercent = 60.0;

        private readonly IFuelStationContext fuelStationContext;

        private readonly ITruckProviderService truckProviderService;

        private readonly FuelStopStationPlanner fuelStopStationPlanner;

        private const double ReferenceWeightLb = 40000.0;
        private const double ReferenceConsumptionGPerKm = 0.089;

        // Расход топлива: 0.10 g/км (10 g/100 км)
        private double TruckFuelConsumptionGPerKm = 0.10;

        // Ёмкость бака: 200 галлонов
        private double TruckTankCapacityG = 200.0 - TankRestrictions;

        // Начальный объём топлива: 60 галлонов
        private  double InitialFuelPercent = 20.0;



        public GetFuelStationsByRoadsQueryHandler(IFuelStationContext fuelStationContext, ITruckProviderService truckProviderService)
        {
            this.fuelStationContext = fuelStationContext;
            this.truckProviderService = truckProviderService;
            this.fuelStopStationPlanner = new FuelStopStationPlanner();
        }

        public async Task<Result<PlanFuelStationsByRoadsResponce>> Handle(
            GetFuelStationsByRoadsQuery request,
            CancellationToken cancellationToken)
        {


            TruckTankCapacityG = TruckTankCapacityG + TankRestrictions;

            InitialFuelPercent = TruckTankCapacityG * (request.CurrentFuel / 100.0);

            TruckTankCapacityG = TruckTankCapacityG - TankRestrictions;
            //var providerTruckId = GetProviderTruckId(Guid.Parse("5f0d3007-707e-4e5f-b46b-ebe4b50b9395"));

            //var currentFuelSams = truckProviderService.GetVehicleStatsFeedAsync(providerTruckId);


            TruckFuelConsumptionGPerKm = CalculateConsumptionGPerKm(request.Weight);


            // 1. Проверка входных данных
            if (request?.Roads == null || !request.Roads.Any(r => r.Points?.Any(p => p?.Count >= 2) == true))
                return Result.Fail("No valid roads or points provided.");

            // 2. Собираем все точки маршрута (в порядке следования)
            var routePoints = request.Roads
                .SelectMany(r => r.Points.Where(p => p?.Count >= 2)
                    .Select(p => new GeoPoint(p[0], p[1])))
                .ToList();

            if (routePoints.Count < 2)
                return Result.Fail("Маршрут содержит менее двух точек.");


            double avgLatRadians = DegreesToRadians(routePoints.Average(pt => pt.Latitude));
            var minLat = routePoints.Min(pt => pt.Latitude) - (SearchRadiusKm / 111.0);
            var maxLat = routePoints.Max(pt => pt.Latitude) + (SearchRadiusKm / 111.0);
            var minLon = routePoints.Min(pt => pt.Longitude) - (SearchRadiusKm / (111.0 * Math.Cos(avgLatRadians)));
            var maxLon = routePoints.Max(pt => pt.Longitude) + (SearchRadiusKm / (111.0 * Math.Cos(avgLatRadians)));

            // 4. Загружаем станции внутри bounding-box
            var stations = await fuelStationContext.FuelStations
                .Include(s => s.FuelPrices)
                .AsNoTracking()
                .Where(s =>
                    s.Coordinates.Latitude >= minLat &&
                    s.Coordinates.Latitude <= maxLat &&
                    s.Coordinates.Longitude >= minLon &&
                    s.Coordinates.Longitude <= maxLon)
                .ToListAsync(cancellationToken);

            if (!stations.Any())
                return Result.Ok(new PlanFuelStationsByRoadsResponce());

            var allStopPlans = new List<FuelStopPlan>();
            var allStationsWithoutAlgo = new List<FuelStationDto>();

            //var requiredStationDtos = new List<RequiredStationDto>(); // заполни по нужде

            FinishInfo finishInfo = new FinishInfo();

            var tasks = request.Roads.Select(road =>
                Task.Run(() =>
                {
                    return PlanRouteStopsForRoad(
                        road,
                        stations,
                        request.RequiredFuelStations,
                        request.FuelProviderNameList,
                        request.FinishFuel);
                })
            ).ToList();

            var results = await Task.WhenAll(tasks);

            foreach (var routeStopsForRoadInfo in results)
            {
                allStopPlans.AddRange(routeStopsForRoadInfo.StopPlan);
                allStationsWithoutAlgo.AddRange(routeStopsForRoadInfo.StationsWithoutAlgorithm);
                finishInfo = routeStopsForRoadInfo.Finish;
            }

            if (!allStopPlans.Any())
                return Result.Ok(new PlanFuelStationsByRoadsResponce { FuelStations = RemoveDuplicatesByCoordinates(allStationsWithoutAlgo) });


            var resultDto = MapStopPlansToDtos(allStopPlans);


            var zipStations = ZipStations(allStationsWithoutAlgo, resultDto);

            //var updatedFuelStations = RemoveDuplicatesByCoordinates(zipStations);
            return Result.Ok(new PlanFuelStationsByRoadsResponce 
            { 
                FuelStations = zipStations,
                FinishInfo = finishInfo
            });
        }

        private List<FuelStationDto> MapStopPlansToDtos(List<FuelStopPlan> allStopPlans)
        {
            var result = new List<FuelStationDto>();

            var groupedBySection = allStopPlans
                .GroupBy(x => x.RoadSectionId)
                .ToList();

            foreach (var group in groupedBySection)
            {
                var sectionStops = group.OrderBy(x => x.StopAtKm).ToList();

                for (int i = 0; i < sectionStops.Count; i++)
                {
                    var current = sectionStops[i];
                    double nextDistanceKm;

                    if (i < sectionStops.Count - 1)
                    {
                        var next = sectionStops[i + 1];
                        nextDistanceKm = next.StopAtKm - current.StopAtKm;
                    }
                    else
                    {
                        nextDistanceKm = 0;
                    }

                    result.Add(FuelStationToDto(
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


        private RouteStopsForRoadInfo PlanRouteStopsForRoad(
            RoadSectionDto road,
            List<FuelStation> allStations,
            List<RequiredStationDto> requiredStationDtos,
            List<string> fuelProviderNameList,
            double finishFuel = 40)
        {
            var routePoints = road.Points
                ?.Where(p => p?.Count >= 2)
                .Select(p => new GeoPoint(p[0], p[1]))
                .ToList() ?? new();

            if (routePoints.Count < 2)
                return new RouteStopsForRoadInfo();

            // Гарантируем, что список имён поставщиков не null:
            fuelProviderNameList = fuelProviderNameList ?? new List<string>();

            // Приводим все элементы к нижнему регистру и отбрасываем пустые:
            var providerFilter = fuelProviderNameList
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.ToLowerInvariant())
                .ToList();

            // Формируем список станций вдоль маршрута:
            var stationsAlongRoute = routePoints
                .SelectMany(geoPoint => allStations
                    .Where(s => GeoCalculator.IsPointWithinRadius(geoPoint, s.Coordinates, SearchRadiusKm)))
                .DistinctBy(s => s.Id)
                // Если providerFilter пуст, пропускаем всех; иначе — фильтруем по совпадению (игнорируя регистр)
                .Where(s => !providerFilter.Any()
                            || providerFilter.Contains(s.ProviderName?.ToLowerInvariant() ?? string.Empty))
                .ToList();

            stationsAlongRoute = RemoveDuplicatesByCoordinates(stationsAlongRoute);

            if (!stationsAlongRoute.Any())
                return new RouteStopsForRoadInfo();

            var fuelstationWithoutAlgorithm = stationsAlongRoute
                .Select(x => FuelStationToDtoNoAlgorithm(x, road.RoadSectionId))
                .ToList();

            double totalRouteDistanceKm = 0;
            for (int i = 0; i < routePoints.Count - 1; i++)
            {
                totalRouteDistanceKm += GeoCalculator.CalculateHaversineDistance(routePoints[i], routePoints[i + 1]);
            }

            var stopPlan = fuelStopStationPlanner.PlanStopsByStations(
                routePoints,
                stationsAlongRoute,
                totalRouteDistanceKm,
                TruckFuelConsumptionGPerKm,
                InitialFuelPercent,
                TruckTankCapacityG,
                requiredStationDtos,
                finishFuel,
                road.RoadSectionId);

            return new RouteStopsForRoadInfo 
            { 
                StopPlan = stopPlan.StopPlan, 
                StationsWithoutAlgorithm = fuelstationWithoutAlgorithm, 
                Finish = stopPlan.Finish 
            };
        }


        private List<FuelStationDto> RemoveDuplicatesByCoordinates(List<FuelStationDto> fuelStations)
        {
            return fuelStations
                .GroupBy(station => (station.Latitude, station.Longitude)) // Group by coordinates
                .Select(group =>
                    group.OrderByDescending(station => station.IsAlgorithm) // Prioritize isAlgorithm: true
                         .First()) // Take the first (true if exists, else false)
                .ToList();
        }

        private List<FuelStation> RemoveDuplicatesByCoordinates(List<FuelStation> fuelStations)
        {
            return fuelStations
                .GroupBy(station => (station.Coordinates.Latitude, station.Coordinates.Longitude)) // Group by coordinates
                .Select(group => group.First()) 
                .ToList();
        }


        private List<FuelStationDto> ZipStations(List<FuelStationDto> fuelstationWithoutAlgorithm, List<FuelStationDto> stopPlan)
        {
            var updatedFuelStations = fuelstationWithoutAlgorithm.Select(station =>
            {
                var matchingStation = stopPlan.FirstOrDefault(s => s.Id == station.Id && s.RoadSectionId == station.RoadSectionId);
                if (matchingStation != null)
                {
                    return new FuelStationDto
                    {
                        Id = station.Id,
                        Name = station.Name,
                        Address = station.Address,
                        Latitude = station.Latitude,
                        Longitude = station.Longitude,
                        Price = station.Price,
                        Discount = station.Discount,
                        PriceAfterDiscount = station.PriceAfterDiscount,
                        IsAlgorithm = matchingStation.IsAlgorithm,
                        Refill = matchingStation.Refill,
                        StopOrder = matchingStation.StopOrder,
                        NextDistanceKm = matchingStation.NextDistanceKm,
                        RoadSectionId = matchingStation.RoadSectionId,
                        CurrentFuel = matchingStation.CurrentFuel
                    };
                }
                return new FuelStationDto
                {
                    Id = station.Id,
                    Name = station.Name,
                    Address = station.Address,
                    Latitude = station.Latitude,
                    Longitude = station.Longitude,
                    Price = station.Price,
                    Discount = station.Discount,
                    PriceAfterDiscount = station.PriceAfterDiscount,
                    IsAlgorithm = station.IsAlgorithm,
                    Refill = station.Refill,
                    StopOrder = station.StopOrder,
                    NextDistanceKm = station.NextDistanceKm,
                    RoadSectionId = station.RoadSectionId,
                    CurrentFuel = station.CurrentFuel
                };
            }).ToList();

            return updatedFuelStations ?? new List<FuelStationDto>();
        }



        private static double CalculateConsumptionGPerKm(double weightLb)
        {
            return ReferenceConsumptionGPerKm * (weightLb / ReferenceWeightLb);
        }

        private FuelStationDto FuelStationToDto(
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


        private FuelStationDto FuelStationToDtoNoAlgorithm(
         FuelStation station,
         string roadSectionId)
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

        private static double DegreesToRadians(double degrees)
            => degrees * (Math.PI / 180.0);
    }

    public class TruckMapping
    {
        public Guid TruckId { get; set; }
        public string ProviderTruckId { get; set; }
    }
}
