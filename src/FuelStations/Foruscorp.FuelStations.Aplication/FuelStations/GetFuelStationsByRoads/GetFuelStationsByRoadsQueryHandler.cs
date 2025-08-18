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
            var stationsAlongRoute = allStations
                .Where(s => routePoints
                    .Any(geoPoint => GeoCalculator.IsPointWithinRadius(geoPoint, s.Coordinates, SearchRadiusKm)))
                .DistinctBy(s => s.Id)
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

            var stopPlan = PlanStopsByStations(
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



        private StopPlanInfo PlanStopsByStations(
            List<GeoPoint> route,
            List<FuelStation> stationsAlongRoute,
            double totalRouteDistanceKm,
            double fuelConsumptionPerKm,
            double currentFuelLiters,
            double tankCapacity,
            List<RequiredStationDto> requiredStops,
            double finishFuel,
            string RoadSectionId = null)
        {
            // Validate finishFuel
            if (finishFuel < 0 || finishFuel > tankCapacity)
                throw new ArgumentException($"finishLiters must be between 0 and tankCapacity ({tankCapacity} liters).");

            // 1) Подготовка списка StationInfo (с километражем и ценой)
            var stationInfos = new List<StationInfo>();
            foreach (var st in stationsAlongRoute)
            {
                double forwardDist = GetForwardDistanceAlongRoute(route, st.Coordinates);
                if (forwardDist < double.MaxValue)
                {
                    var cheapestFuelPrice = st.FuelPrices
                        .Where(fp => fp.PriceAfterDiscount >= 0)
                        .OrderBy(fp => fp.PriceAfterDiscount)
                        .FirstOrDefault();

                    double pricePerLiter = cheapestFuelPrice?.PriceAfterDiscount ?? double.MaxValue;

                    stationInfos.Add(new StationInfo
                    {
                        Station = st,
                        ForwardDistanceKm = forwardDist,
                        PricePerLiter = pricePerLiter
                    });
                }
            }

            var endInfo = new StationInfo
            {
                Station = null,
                ForwardDistanceKm = totalRouteDistanceKm,
                PricePerLiter = 0.0
            };
            stationInfos.Add(endInfo);

            // 2) Собираем обязательные остановки и сортируем по километражу
            var requiredInfos = requiredStops
                .Select(r =>
                {
                    var si = stationInfos.FirstOrDefault(x => x.Station != null && x.Station.Id == r.StationId);
                    if (si == null) return null;
                    return new
                    {
                        Info = si,
                        RefillLiters = Math.Min(r.RefillLiters, tankCapacity)
                    };
                })
                .Where(x => x != null)
                .OrderBy(x => x.Info.ForwardDistanceKm)
                .ToList();

            var result = new List<FuelStopPlan>();
            var usedStationIds = new HashSet<Guid>();
            double prevKm = 0.0;
            double remainingFuel = currentFuelLiters;

            bool isFirstStop = true;

            // Вспомогательный метод: планирует промежуточные дозаправки, пока не достигнем targetKm
            void PlanTill(double targetKm, bool useMinDistance = false, double minStopDistanceKm = 1200.0)
            {
                // Сколько топлива надо, чтобы доехать от prevKm до targetKm, с учётом finishFuel для финального сегмента
                double neededFuel = (targetKm - prevKm) * fuelConsumptionPerKm + (targetKm == totalRouteDistanceKm ? finishFuel : 0);

                double extraRange = (tankCapacity - TankRestrictions) / fuelConsumptionPerKm;
                //double normalRange = tankCapacity / fuelConsumptionPerKm;

                while (remainingFuel < neededFuel)
                {
                    double maxDistanceWithoutRefuel = remainingFuel / fuelConsumptionPerKm;
                    double maxReachKm = prevKm + maxDistanceWithoutRefuel;

                    // Первая попытка: с учётом minStopDistanceKm
                    var candidates = stationInfos
                        .Where(si =>
                            si.Station != null &&
                            !usedStationIds.Contains(si.Station.Id) &&
                            si.ForwardDistanceKm > prevKm &&
                            (
                                // экстренно, если до minStopDistance топлива не хватает,
                                // или станция удовлетворяет минимальную дистанцию
                                maxDistanceWithoutRefuel < minStopDistanceKm ||
                                si.ForwardDistanceKm - prevKm >= minStopDistanceKm
                            ) &&
                            si.ForwardDistanceKm <= maxReachKm
                        )
                        .OrderBy(si => si.PricePerLiter)
                        .ToList();

                    // Если кандидатов нет, игнорируем minStopDistanceKm

                    //while(!candidates.Any() && minStopDistanceKm > 0)
                    //{
                    //    if(minStopDistanceKm > 400)
                    //        minStopDistanceKm -= 50;

                    //    candidates = stationInfos
                    //    .Where(si =>
                    //        si.Station != null &&
                    //        !usedStationIds.Contains(si.Station.Id) &&
                    //        si.ForwardDistanceKm > prevKm &&
                    //        (

                    //            // экстренно, если до minStopDistance топлива не хватает,
                    //            // или станция удовлетворяет минимальную дистанцию

                    //            maxDistanceWithoutRefuel < minStopDistanceKm ||
                    //            si.ForwardDistanceKm - prevKm >= minStopDistanceKm
                    //        ) &&
                    //        si.ForwardDistanceKm <= maxReachKm
                    //    )
                    //    .OrderBy(si => si.PricePerLiter)
                    //    .ToList();
                    //}

                    //minStopDistanceKm = 1200;

                    if (!candidates.Any() && useMinDistance)
                    {
                        candidates = stationInfos
                            .Where(si =>
                                si.Station != null &&
                                !usedStationIds.Contains(si.Station.Id) &&
                                si.ForwardDistanceKm > prevKm &&
                                si.ForwardDistanceKm <= maxReachKm
                            )
                            .OrderBy(si => si.PricePerLiter)
                            .ToList();
                    }

                    // Debug: log if no candidates (optional, can be removed)
                    if (!candidates.Any())
                    {
                        // For debugging, you can log or inspect:
                        // Console.WriteLine($"No candidates at {prevKm}km, maxReachKm={maxReachKm}, neededFuel={neededFuel}, remainingFuel={remainingFuel}");
                        // Fallback: proceed with closest possible solution
                        break;
                    }

                    var best = candidates.First();

                    // Доезжаем до best
                    double dist = best.ForwardDistanceKm - prevKm;
                    remainingFuel -= dist * fuelConsumptionPerKm;

                    double preRemainingFuel = remainingFuel;

                    bool isLastRefuel = best.ForwardDistanceKm + extraRange >= totalRouteDistanceKm;

                    //double effectiveCapacity = isFirstStop
                    //   ? tankCapacity + TankRestrictions
                    //   : tankCapacity;

                    if (isLastRefuel)
                        Console.WriteLine(      );


                    double effectiveCapacity = (isFirstStop || isLastRefuel)
                       ? tankCapacity + TankRestrictions
                       : tankCapacity;

                    // Дозаправка: для последнего сегмента учитываем finishFuel
                    double freeSpace = effectiveCapacity - remainingFuel;
                    double rawRefill;

                    if (targetKm == totalRouteDistanceKm && (remainingFuel >= neededFuel) /*(targetKm - best.ForwardDistanceKm) * fuelConsumptionPerKm)*/ /*&& best.ForwardDistanceKm < targetKm*/)
                    {
                        // Для финального сегмента рассчитываем, чтобы осталось ровно finishFuel
                        double fuelToTarget = (targetKm - best.ForwardDistanceKm) * fuelConsumptionPerKm;
                        rawRefill = fuelToTarget + finishFuel - remainingFuel;
                    }
                    else
                    {
                        // Для промежуточных остановок заливаем кратно 5 литрам
                        rawRefill = freeSpace;
                    }

                    double refill = Math.Floor(rawRefill / 5.0) * 5.0;

                    if (refill == 0 && freeSpace >= 5.0)
                        refill = 5.0;
                    else if (refill == 0 && freeSpace < 5.0)
                        refill = freeSpace; // мелкий остаток, зальём его

                    // Проверяем, что дозаправка не превышает доступное место в баке
                    refill = Math.Min(refill, freeSpace);

                    if (refill < 0)
                    {
                        // Console.WriteLine($"Negative refill at {best.ForwardDistanceKm}km, rawRefill={rawRefill}");
                        refill = freeSpace; // Fallback to fill tank
                    }

                    remainingFuel += refill;

                    result.Add(new FuelStopPlan
                    {
                        Station = best.Station!,
                        StopAtKm = best.ForwardDistanceKm,
                        RefillLiters = refill,
                        CurrentFuelLiters = preRemainingFuel,
                        RoadSectionId = RoadSectionId ?? string.Empty
                    });
                    usedStationIds.Add(best.Station!.Id);
                    prevKm = best.ForwardDistanceKm;

                    if (isFirstStop)
                        isFirstStop = false;

                    // Пересчитаем, сколько ещё топлива нужно
                    neededFuel = (targetKm - prevKm) * fuelConsumptionPerKm + (targetKm == totalRouteDistanceKm ? finishFuel : 0);
                }

                // «Проезжаем» до targetKm
                remainingFuel -= (targetKm - prevKm) * fuelConsumptionPerKm;
                prevKm = targetKm;
            }

            // 3) Для каждого обязательного сегмента: сначала промежуточные, затем обязательная
            foreach (var req in requiredInfos)
            {
                double kmReq = req.Info.ForwardDistanceKm;

                // Дозаправки между prevKm и обязательной (игнорируем MinStopDistance)
                PlanTill(kmReq);

                //      double toRefill = Math.Min(req.RefillLiters,
                //(isFirstStop ? tankCapacity + TankRestrictions : tankCapacity) - remainingFuel);

                // Теперь делаем саму обязательную дозаправку ровно req.RefillLiters
                double allowed = Math.Min(req.RefillLiters, tankCapacity - remainingFuel);

                double preRemainingFuel = remainingFuel;

                remainingFuel += allowed;
                result.Add(new FuelStopPlan
                {
                    Station = req.Info.Station!,
                    StopAtKm = kmReq,
                    RefillLiters = allowed,
                    CurrentFuelLiters = preRemainingFuel,
                    RoadSectionId = RoadSectionId ?? string.Empty
                });
                usedStationIds.Add(req.Info.Station!.Id);
                // prevKm уже == kmReq
            }

            // 4) Наконец, от последней обязательной до конца маршрута 
            PlanTill(totalRouteDistanceKm, useMinDistance: true);

            // 5) Корректируем последнюю заправку, если remainingFuel != finishFuel
            if (Math.Abs(remainingFuel - finishFuel) > 0.0001 && result.Any())
            {
                // Берем последнюю заправку
                var lastStop = result.Last();
                double fuelToFinish = (totalRouteDistanceKm - lastStop.StopAtKm) * fuelConsumptionPerKm;
                double requiredRefill = fuelToFinish + finishFuel - lastStop.CurrentFuelLiters;
                requiredRefill = Math.Min(requiredRefill, tankCapacity + 40 - lastStop.CurrentFuelLiters);

                if (requiredRefill >= 0)
                {
                    lastStop.RefillLiters = requiredRefill;
                    remainingFuel = lastStop.CurrentFuelLiters + requiredRefill - fuelToFinish;
                }
            }

            // 6) Формируем объект финиша
            var finishInfo = new FinishInfo
            {
                RemainingFuelLiters = remainingFuel,
                //Coordinates = route.Last()
            };

            return new StopPlanInfo { StopPlan = result, Finish = finishInfo };
        }

        private static double CalculateConsumptionGPerKm(double weightLb)
        {
            return ReferenceConsumptionGPerKm * (weightLb / ReferenceWeightLb);
        }

        private double GetForwardDistanceAlongRoute(List<GeoPoint> route, GeoPoint stationCoords)
        {
            double cumulative = 0.0;
            for (int i = 0; i < route.Count - 1; i++)
            {
                var a = route[i];
                var b = route[i + 1];
                double segmentLength = GeoCalculator.CalculateHaversineDistance(a, b);
                double distToSegment = GeoCalculator.DistanceFromPointToSegmentKm(stationCoords, a, b);
                if (distToSegment <= SearchRadiusKm)
                {
                    // Считаем проекцию станции на этот отрезок
                    double projectionKm = GeoCalculator.DistanceAlongSegment(a, b, stationCoords);
                    return cumulative + projectionKm;
                }
                cumulative += segmentLength;
            }
            return double.MaxValue; // Станция «не попала» ни в один сегмент коридора
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


        public class StopPlanInfo
        {
            public List<FuelStopPlan> StopPlan { get; set; }
            public FinishInfo Finish { get; set; }
        }

        public class RouteStopsForRoadInfo
        {
            public List<FuelStopPlan> StopPlan { get; set; } = new List<FuelStopPlan>();
            public List<FuelStationDto> StationsWithoutAlgorithm { get; set; } = new List<FuelStationDto>();
            public FinishInfo Finish { get; set; }
        }


        public class FinishInfo
        {
            public double RemainingFuelLiters { get; set; }
            //public GeoPoint Coordinates { get; set; }
        }

        private class StationInfo
        {
            public FuelStation? Station { get; set; }
            public double ForwardDistanceKm { get; set; }
            public double PricePerLiter { get; set; }
        }

        public class RequiredStationDto
        {
            public Guid StationId { get; set; }
            public double RefillLiters { get; set; }
        }


        private class StationPriceComparer : IComparer<StationInfo>
        {
            public int Compare(StationInfo? x, StationInfo? y)
            {
                if (x == null || y == null) return 0;
                int cmp = x.PricePerLiter.CompareTo(y.PricePerLiter);
                if (cmp != 0) return cmp;
                return x.ForwardDistanceKm.CompareTo(y.ForwardDistanceKm);
            }
        }


        private static double DegreesToRadians(double degrees)
            => degrees * (Math.PI / 180.0);
    }

    public class FuelStopPlan
    {
        public FuelStation Station { get; set; } = null!;

        public double StopAtKm { get; set; }

        public double RefillLiters { get; set; }

        public double CurrentFuelLiters { get; set; }

        public string RoadSectionId { get; set; }
    }

    public class TruckMapping
    {
        public Guid TruckId { get; set; }
        public string ProviderTruckId { get; set; }
    }

    public class FuelStationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Latitude { get; set; } = null!;
        public string Longitude { get; set; } = null!;
        public string Price { get; set; } = null!;
        public string? Discount { get; set; }
        public string? PriceAfterDiscount { get; set; }

        public bool IsAlgorithm { get; set; } 

        public string Refill { get; set; } = null!;

        public int StopOrder { get; set; }

        public string NextDistanceKm { get; set; } = null!;

        public string RoadSectionId { get; set; }

        public double CurrentFuel { get; set; } = 0.0; 
    }
}
