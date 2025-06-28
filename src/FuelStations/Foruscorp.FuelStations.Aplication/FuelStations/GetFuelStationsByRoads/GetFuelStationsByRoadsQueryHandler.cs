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

    public class GetFuelStationsByRoadsResponce
    {
        public List<FuelStationDto> FuelStations { get; set; } = new List<FuelStationDto>();
        public FinishInfo FinishInfo { get; set; } = new FinishInfo();  
    }


    public class GetFuelStationsByRoadsQueryHandler :
        IRequestHandler<GetFuelStationsByRoadsQuery, Result<GetFuelStationsByRoadsResponce>>
    {
        // Радиус «коридора» вдоль маршрута (в км), в пределах которого принимаем станции
        private const double SearchRadiusKm = 9.0;



        public const double ExtraCapacity = 40.0;
        private const double MinStopDistanceKm = 1200.0;



        private const double TruckFuelConsumptionLPerKm = 0.10;

        // Ёмкость бака: 200 галлонов
        private double TruckTankCapacityL = 200.0 - 40.0;

        // Начальный объём топлива: 60 галлонов
        private  double InitialFuelLiters = 20.0;

        private readonly IFuelStationContext fuelStationContext;

        private readonly ITruckProviderService truckProviderService;

        public GetFuelStationsByRoadsQueryHandler(IFuelStationContext fuelStationContext, ITruckProviderService truckProviderService)
        {
            this.fuelStationContext = fuelStationContext;
            this.truckProviderService = truckProviderService;
        }

        public async Task<Result<GetFuelStationsByRoadsResponce>> Handle(
            GetFuelStationsByRoadsQuery request,
            CancellationToken cancellationToken)
        {


            TruckTankCapacityL = TruckTankCapacityL + 40.0;

            InitialFuelLiters = TruckTankCapacityL * (request.CurrentFuel / 100.0);

            TruckTankCapacityL = TruckTankCapacityL - 40.0;
            //var providerTruckId = GetProviderTruckId(Guid.Parse("5f0d3007-707e-4e5f-b46b-ebe4b50b9395"));

            //var currentFuelSams = truckProviderService.GetVehicleStatsFeedAsync(providerTruckId);


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
                return Result.Ok(new GetFuelStationsByRoadsResponce());

            var allStopPlans = new List<FuelStopPlan>();
            var allStationsWithoutAlgo = new List<FuelStationDto>();

            //var requiredStationDtos = new List<RequiredStationDto>(); // заполни по нужде

            FinishInfo finishInfo = new FinishInfo();

            foreach (var road in request.Roads)
            {
                var routeStopsForRoadInfo = PlanRouteStopsForRoad(road, stations, request.RequiredFuelStations, request.FuelProviderNameList, request.FinishFuel);

                allStopPlans.AddRange(routeStopsForRoadInfo.StopPlan);
                allStationsWithoutAlgo.AddRange(routeStopsForRoadInfo.StationsWithoutAlgorithm);

                finishInfo = routeStopsForRoadInfo.Finish;
            }

            if (!allStopPlans.Any())
                return Result.Ok(new GetFuelStationsByRoadsResponce { FuelStations = RemoveDuplicatesByCoordinates(allStationsWithoutAlgo) });


            var resultDto = MapStopPlansToDtos(allStopPlans);


            var zipStations = ZipStations(allStationsWithoutAlgo, resultDto);

            //var updatedFuelStations = RemoveDuplicatesByCoordinates(zipStations);
            return Result.Ok(new GetFuelStationsByRoadsResponce 
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

            var stopPlan = PlanStopsByStations(
                routePoints,
                stationsAlongRoute,
                totalRouteDistanceKm,
                TruckFuelConsumptionLPerKm,
                InitialFuelLiters,
                TruckTankCapacityL,
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
            double totalDistanceKm,
            double consumptionPerKm,
            double initialFuel,
            double tankCapacity,
            List<RequiredStationDto> requiredStops,
            double finishFuel,
            string roadSectionId = null)
        {
            // 1. Validate inputs
            if (finishFuel < 0 || finishFuel > tankCapacity)
                throw new ArgumentException($"finishFuel must be between 0 and tankCapacity ({tankCapacity})");

            // 2. Build station info list (distance + cheapest price)
            var infos = GetStationInfos(route, stationsAlongRoute)
                .Append(new StationInfo(null, totalDistanceKm, 0))
                .ToList();

            // 3. Prepare required stops sorted by position
            var requiredInfos = requiredStops
                .Select(r => new { Refill = Math.Min(r.RefillLiters, tankCapacity), Info = infos.FirstOrDefault(si => si.Station?.Id == r.StationId) })
                .Where(x => x.Info != null)
                .OrderBy(x => x.Info.ForwardDistanceKm)
                .Select(x => new RequiredStopInfo(x.Info, x.Refill))
                .ToList();

            // 4. Initialize planner state
            var state = new PlannerState(initialFuel, tankCapacity, totalDistanceKm, consumptionPerKm, finishFuel, roadSectionId);
            var plan = new List<FuelStopPlan>();
            var usedStations = new HashSet<Guid>();

            // 5. Plan stops for each required segment
            foreach (var req in requiredInfos)
            {
                PlanIntermediateStops(infos, plan, usedStations, state, req.Info.ForwardDistanceKm, requireMinDistance: false);
                PlanRequiredStop(plan, usedStations, state, req);
            }

            // 6. Final segment to destination
            PlanIntermediateStops(infos, plan, usedStations, state, totalDistanceKm, requireMinDistance: true);
            AdjustLastRefill(plan, state);

            return new StopPlanInfo
            {
                StopPlan = plan,
                Finish = state.ToFinishInfo()
            };
        }

        private IEnumerable<StationInfo> GetStationInfos(List<GeoPoint> route, List<FuelStation> stations)
        {
            List<StationInfo> infos = new List<StationInfo>();  

            foreach (var st in stations)
            {
                var dist = GetForwardDistanceAlongRoute(route, st.Coordinates);
                if (dist < double.MaxValue)
                {
                    // Получаем минимальную цену или double.MaxValue, если нет доступных цен
                    var price = st.FuelPrices
                        .Where(fp => fp.PriceAfterDiscount >= 0)
                        .Select(fp => fp.PriceAfterDiscount)
                        .DefaultIfEmpty(double.MaxValue)
                        .Min();
                    yield return new StationInfo(st, dist, price);
                }
            }
        }

        private void PlanIntermediateStops(
            List<StationInfo> infos,
            List<FuelStopPlan> plan,
            HashSet<Guid> used,
            PlannerState state,
            double targetKm,
            bool requireMinDistance)
        {
            while (state.NeedsFuelBefore(targetKm))
            {
                var candidate = SelectStation(infos, used, state, requireMinDistance);
                if (candidate == null) break;
                MakeStop(plan, used, state, candidate, targetKm);
            }
            state.MoveTo(targetKm);
        }

        private StationInfo SelectStation(
            List<StationInfo> infos,
            HashSet<Guid> used,
            PlannerState s,
            bool requireMin)
        {
            var reach = s.CurrentKm + s.RemainingRangeKm;
            var query = infos.Where(si => si.Station != null
                && si.ForwardDistanceKm > s.CurrentKm
                && si.ForwardDistanceKm <= reach
                && !used.Contains(si.Station.Id));

            if (requireMin)
            {
                var q2 = query.Where(si => si.ForwardDistanceKm - s.CurrentKm >= MinStopDistanceKm);
                if (q2.Any()) query = q2;
            }

            return query.OrderBy(si => si.PricePerLiter).FirstOrDefault();
        }

        private void MakeStop(
            List<FuelStopPlan> plan,
            HashSet<Guid> used,
            PlannerState s,
            StationInfo si,
            double targetKm)
        {
            // drive to station
            var distance = si.ForwardDistanceKm - s.CurrentKm;
            s.Consume(distance);

            // determine refill amount
            bool isLast = si.ForwardDistanceKm + (s.TankCapacity - ExtraCapacity) / s.ConsumptionPerKm >= s.TotalDistanceKm;
            double cap = s.GetCapacity(isFirst: plan.Count == 0, isLast);
            double needed = isLast
                ? (targetKm - si.ForwardDistanceKm) * s.ConsumptionPerKm + s.FinishFuel - s.RemainingFuel
                : cap - s.RemainingFuel;

            double refill = Math.Floor(needed / 5) * 5;
            refill = Math.Clamp(refill, Math.Min(5, needed), cap - s.RemainingFuel);

            // perform refill
            var before = s.RemainingFuel;
            s.AddFuel(refill);

            plan.Add(new FuelStopPlan
            {
                Station = si.Station,
                StopAtKm = si.ForwardDistanceKm,
                RefillLiters = refill,
                CurrentFuelLiters = before,
                RoadSectionId = s.RoadSectionId
            });
            used.Add(si.Station.Id);
        }

        private void PlanRequiredStop(
            List<FuelStopPlan> plan,
            HashSet<Guid> used,
            PlannerState s,
            RequiredStopInfo req)
        {
            s.MoveTo(req.Info.ForwardDistanceKm);
            var space = s.TankCapacity - s.RemainingFuel;
            var refill = Math.Min(req.RefillLiters, space);
            var before = s.RemainingFuel;
            s.AddFuel(refill);

            plan.Add(new FuelStopPlan
            {
                Station = req.Info.Station,
                StopAtKm = req.Info.ForwardDistanceKm,
                RefillLiters = refill,
                CurrentFuelLiters = before,
                RoadSectionId = s.RoadSectionId
            });
            used.Add(req.Info.Station.Id);
        }

        private void AdjustLastRefill(List<FuelStopPlan> plan, PlannerState s)
        {
            if (!plan.Any()) return;
            // ensure exact finishFuel
            var last = plan.Last();
            var needed = (s.TotalDistanceKm - last.StopAtKm) * s.ConsumptionPerKm + s.FinishFuel;
            var delta = needed - (last.CurrentFuelLiters + last.RefillLiters);
            if (Math.Abs(delta) > 0.001)
            {
                last.RefillLiters = Math.Clamp(last.RefillLiters + delta, 0, s.TankCapacity);
            }
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

    // Helper classes
    public class StationInfo
    {
        public FuelStation Station { get; }
        public double ForwardDistanceKm { get; }
        public double PricePerLiter { get; }
        public StationInfo(FuelStation st, double d, double p)
        {
            Station = st;
            ForwardDistanceKm = d;
            PricePerLiter = p;
        }
    }

    public class RequiredStopInfo
    {
        public StationInfo Info { get; }
        public double RefillLiters { get; }
        public RequiredStopInfo(StationInfo info, double refill)
        {
            Info = info;
            RefillLiters = refill;
        }
    }

    public class PlannerState
    {
        public double CurrentKm { get; private set; }
        public double RemainingFuel { get; private set; }
        public double TankCapacity { get; }
        public double ConsumptionPerKm { get; }
        public double FinishFuel { get; }
        public double TotalDistanceKm { get; }
        public string RoadSectionId { get; }

        public PlannerState(double fuel, double cap, double totalKm, double cons, double finish, string roadId)
        {
            RemainingFuel = fuel;
            TankCapacity = cap;
            TotalDistanceKm = totalKm;
            ConsumptionPerKm = cons;
            FinishFuel = finish;
            RoadSectionId = roadId;
        }

        public bool NeedsFuelBefore(double targetKm)
        {
            var needed = (targetKm - CurrentKm) * ConsumptionPerKm + (targetKm == TotalDistanceKm ? FinishFuel : 0);
            return RemainingFuel < needed;
        }

        public void Consume(double distance)
        {
            RemainingFuel -= distance * ConsumptionPerKm;
            CurrentKm += distance;
        }

        public void AddFuel(double liters)
        {
            RemainingFuel += liters;
        }

        public double RemainingRangeKm => RemainingFuel / ConsumptionPerKm;

        public double GetCapacity(bool isFirst, bool isLast)
            => TankCapacity + ((isFirst || isLast) ? ExtraCapacity : 0);

        public void MoveTo(double km)
        {
            var dist = km - CurrentKm;
            RemainingFuel -= dist * ConsumptionPerKm;
            CurrentKm = km;
        }

        public FinishInfo ToFinishInfo()
            => new FinishInfo { RemainingFuelLiters = RemainingFuel };
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
