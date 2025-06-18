using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Domain.FuelStations;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class GetFuelStationsByRoadsQueryHandler :
        IRequestHandler<GetFuelStationsByRoadsQuery, Result<List<FuelStationDto>>>
    {
        // Радиус «коридора» вдоль маршрута (в км), в пределах которого принимаем станции
        private const double SearchRadiusKm = 20.0;


        //// Расход топлива: 0.3 л/км (30 л/100 км)
        //private const double TruckFuelConsumptionLPerKm = 0.3;

        //// Ёмкость бака в галонах
        //private const double TruckTankCapacityL = 200.0;

        //// Начальный объём топлива в галонах (можно брать из запроса, здесь для примера)
        //private const double InitialFuelLiters = 60.0;



        private const double TruckFuelConsumptionLPerKm = 0.10;

        // Ёмкость бака: 200 галлонов
        private const double TruckTankCapacityL = 200.0 - 40.0;

        // Начальный объём топлива: 60 галлонов
        private const double InitialFuelLiters = 20.0;

        private readonly IFuelStationContext fuelStationContext;

        public GetFuelStationsByRoadsQueryHandler(IFuelStationContext fuelStationContext)
        {
            this.fuelStationContext = fuelStationContext;
        }

        public async Task<Result<List<FuelStationDto>>> Handle(
            GetFuelStationsByRoadsQuery request,
            CancellationToken cancellationToken)
        {
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
                return Result.Ok(new List<FuelStationDto>()); 

            var stationsAlongRoute = request.Roads
                .SelectMany(r => r.Points?.Where(p => p?.Count >= 2) ?? Enumerable.Empty<List<double>>())
                .Select(p => new GeoPoint(p[0], p[1]))
                .SelectMany(geoPoint => stations
                    .Where(s => GeoCalculator.IsPointWithinRadius(geoPoint, s.Coordinates, SearchRadiusKm)))
                .DistinctBy(s => s.Id)
                //.DistinctBy(s => s.Address)
                .ToList();

            if (!stationsAlongRoute.Any())
                return Result.Ok(new List<FuelStationDto>());

            var stationsAlongFirstRout = request.Roads.FirstOrDefault().Points
                 ?.Where(p => p?.Count >= 2)
                 .Select(p => new GeoPoint(p[0], p[1]))
                 .SelectMany(geoPoint => stations
                     .Where(s => GeoCalculator.IsPointWithinRadius(geoPoint, s.Coordinates, SearchRadiusKm)))
                    .DistinctBy(s => s.Id)
                   //.DistinctBy(s => s.Address)
                 .ToList();

            var fuelstationWithoutAlgorithm = stationsAlongFirstRout.Select(x => FuelStationToDtoNoAlgorithm(x));
            //var fuelstationWithoutAlgorithm = stationsAlongRoute.Select(x => FuelStationToDtoNoAlgorithm(x));


            // 6. Считаем общую длину маршрута (в км)
            var route = request.Roads.FirstOrDefault().Points.Select(p => new GeoPoint(p[0], p[1])).ToList();
            double totalRouteDistanceKm = 0;
            for (int i = 0; i < route.Count - 1; i++)
            {
                totalRouteDistanceKm += GeoCalculator.CalculateHaversineDistance(route[i], route[i + 1]);
            }

            var requiredStationDtos = new List<RequiredStationDto>() {
                //new RequiredStationDto
                //{
                //    StationId = Guid.Parse("15640422-6095-4f4a-bfc4-4331e6c0859e"),
                //    RefillLiters = 35.0
                //},
                //new RequiredStationDto
                //{
                //    StationId = Guid.Parse("35cf17ba-e66b-4e53-9a54-44dcefed8e27"),
                //    RefillLiters = 20.0
                //},
                //new RequiredStationDto
                //{
                //    StationId = Guid.Parse("2e8ae0ca-74c0-4c19-9883-ee0998ab3c7b"),
                //    RefillLiters = 150.0
                //}


            };

            // 7. Запускаем алгоритм «по заправкам» с оптимизацией по цене
            var stopPlan = PlanStopsByStations(
                routePoints,
                stationsAlongFirstRout,
                totalRouteDistanceKm,
                TruckFuelConsumptionLPerKm,
                InitialFuelLiters,
                TruckTankCapacityL,
                requiredStationDtos);


            var resultDto = new List<FuelStationDto>();
            for (int i = 0; i < stopPlan.Count; i++)
            {
                var current = stopPlan[i];
                double nextDistanceKm;

                if (i < stopPlan.Count - 1)
                {
                    // расстояние между этой остановкой и следующей
                    var next = stopPlan[i + 1];
                    nextDistanceKm = next.StopAtKm - current.StopAtKm;
                }
                else
                {
                    // для последней: от неё до конца маршрута
                    nextDistanceKm = totalRouteDistanceKm - current.StopAtKm;
                }

                resultDto.Add(FuelStationToDto(
                    current.Station,
                    stopOrder: i + 1,
                    refillLiters: current.RefillLiters,
                    nextDistanceKm: nextDistanceKm));
            }

            var updatedFuelStations = RemoveDuplicatesByCoordinates(ZipStations(fuelstationWithoutAlgorithm.ToList(), resultDto));

            return Result.Ok(updatedFuelStations);
        }

        private List<FuelStationDto> RemoveDuplicatesByAddress(List<FuelStationDto> fuelStations)
        {
            return fuelStations
                .GroupBy(station => station.Address) // Group by address
                .Select(group =>
                    group.OrderByDescending(station => station.IsAlgorithm) // Prioritize isAlgorithm: true
                         .First()) // Take the first (true if exists, else false)
                .ToList();
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

        private List<FuelStationDto> ZipStations(List<FuelStationDto> fuelstationWithoutAlgorithm, List<FuelStationDto> stopPlan)
        {

            var updatedFuelStations = fuelstationWithoutAlgorithm.Select(station =>
            {
                var matchingStation = stopPlan.FirstOrDefault(s => s.Id == station.Id);
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
                        NextDistanceKm = matchingStation.NextDistanceKm
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
                    NextDistanceKm = station.NextDistanceKm
                };
            }).ToList();

            return updatedFuelStations ?? new List<FuelStationDto>();
        }


        private List<FuelStopPlan> PlanStopsByStations(
            List<GeoPoint> route,
            List<FuelStation> stationsAlongRoute,
            double totalRouteDistanceKm,
            double fuelConsumptionPerKm,
            double currentFuelLiters,
            double tankCapacity,
            List<RequiredStationDto> requiredStops)
        {
            const double MinStopDistanceKm = 1200.0;  // минимальный пробег между остановками

            // 1) Подготовка списка StationInfo (с километражем и ценой)
            var stationInfos = new List<StationInfo>();
            foreach (var st in stationsAlongRoute)
            {
                double forwardDist = GetForwardDistanceAlongRoute(route, st.Coordinates);
                if (forwardDist < double.MaxValue)
                {
                    // Берём самую низкую цену после скидки у станции
                    var cheapestFuelPrice = st.FuelPrices
                        .Where(fp => fp.PriceAfterDiscount >= 0)         // отфильтровываем некорректные
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
                    var si = stationInfos.FirstOrDefault(x => x.Station.Id == r.StationId);
                    if (si == null) return null;
                    return new
                    {
                        Info = si,
                        RefillLiters = Math.Min(r.RefillLiters, tankCapacity)
                    };
                })
                .Where(x => x != null)
                .OrderBy(x => x.Info.ForwardDistanceKm)
                .ToList()!;

            var result = new List<FuelStopPlan>();
            var usedStationIds = new HashSet<Guid>();
            double prevKm = 0.0;
            double remainingFuel = currentFuelLiters;


            // Вспомогательный метод: планирует промежуточные дозаправки, пока не достигнем targetKm
            void PlanTill(double targetKm, bool useMinDistance)
            {

                // сколько топлива надо, чтобы доехать от prevKm до targetKm
                double neededFuel = (targetKm - prevKm) * fuelConsumptionPerKm;

                

                while (remainingFuel < neededFuel)
                {
                    double maxDistanceWithoutRefuel = remainingFuel / fuelConsumptionPerKm;

                    double maxReachKm = prevKm + (remainingFuel / fuelConsumptionPerKm);

                    var candidates = stationInfos
                        .Where(si =>
                         si.Station != null &&
                            !usedStationIds.Contains(si.Station.Id) &&
                            si.ForwardDistanceKm > prevKm &&
                            (
                                 //useMinDistance ||                                     
                                 maxDistanceWithoutRefuel < MinStopDistanceKm ||      
                                 si.ForwardDistanceKm - prevKm >= MinStopDistanceKm 
                            )&&
                            si.ForwardDistanceKm <= maxReachKm 
                        )

                        //(useMinDistance || /*(maxReachKm >= MinStopDistanceKm &&*/ si.ForwardDistanceKm - prevKm >= MinStopDistanceKm))
                        .OrderBy(si => si.PricePerLiter)
                        //.OrderByDescending(si => si.ForwardDistanceKm)
                        //.ThenBy(si => si.PricePerLiter)
                        //.ThenByDescending(si => si.ForwardDistanceKm)
                        .ToList();

                    if (!candidates.Any())
                        break; // нечем дозаправиться

                    var best = candidates.First();

                    // доезжаем до best
                    double dist = best.ForwardDistanceKm - prevKm;
                    remainingFuel -= dist * fuelConsumptionPerKm;

                    double preRemainingFuel = remainingFuel;

                    // дозаправка: минимум 30L или до полного бака
                    //double toFull = tankCapacity - remainingFuel;

                    // новый вариант – refill кратен 5 литрам
                    double freeSpace = tankCapacity - remainingFuel;
                    double rawRefill = freeSpace;

                    double refill = Math.Floor(rawRefill / 5.0) * 5.0;

                    if (refill == 0 && freeSpace >= 5.0)
                        refill = 5.0;
                    else if (refill == 0 && freeSpace < 5.0)
                        refill = freeSpace; // мелкий остаток, зальём его

                    //double refill = Math.Min(Math.Max(toFull, 30.0), toFull);
                    remainingFuel += refill;

                    result.Add(new FuelStopPlan
                    {
                        Station = best.Station!,
                        StopAtKm = best.ForwardDistanceKm,
                        RefillLiters = refill,
                        CurrentFuelLiters = preRemainingFuel
                    });
                    usedStationIds.Add(best.Station!.Id);
                    prevKm = best.ForwardDistanceKm;

                    // пересчитаем, сколько ещё топлива нужно
                    neededFuel = (targetKm - prevKm) * fuelConsumptionPerKm;
                }

                // «проезжаем» до targetKm
                remainingFuel -= (targetKm - prevKm) * fuelConsumptionPerKm;
                prevKm = targetKm;
            }

            // 3) Для каждого обязательного сегмента: сначала промежуточные, затем обязательная
            foreach (var req in requiredInfos)
            {
                double kmReq = req.Info.ForwardDistanceKm;

                // дозаправки между prevKm и обязательной (игнорируем MinStopDistance)
                PlanTill(kmReq, useMinDistance: true);

                // теперь делаем саму обязательную дозаправку ровно req.RefillLiters
                double allowed = Math.Min(req.RefillLiters, tankCapacity - remainingFuel);

                double preRemainingFuel = remainingFuel;

                remainingFuel += allowed;
                result.Add(new FuelStopPlan
                {
                    Station = req.Info.Station!,
                    StopAtKm = kmReq,
                    RefillLiters = allowed,
                    CurrentFuelLiters = preRemainingFuel
                });
                usedStationIds.Add(req.Info.Station!.Id);
                // prevKm уже == kmReq
            }

            // 4) Наконец, от последней обязательной до конца маршрута 
            PlanTill(totalRouteDistanceKm, useMinDistance: false);

            return result;
        }

        //private List<FuelStopPlan> PlanStopsByStations(
        //    List<GeoPoint> route,
        //    List<FuelStation> stationsAlongRoute,
        //    double totalRouteDistanceKm,
        //    double fuelConsumptionPerKm,
        //    double currentFuelLiters,
        //    double tankCapacity,
        //    List<RequiredStationDto> requiredStops)    // ← теперь принимаем список обязательных
        //{
        //    // 1) Подготовка stationInfos (цены и километраж по маршруту)
        //    var stationInfos = stationsAlongRoute
        //        .Select(st =>
        //        {
        //            double km = GetForwardDistanceAlongRoute(route, st.Coordinates);
        //            // если нет цены, ставим MaxValue
        //            double price = st.FuelPrices
        //                .Where(fp => fp.PriceAfterDiscount >= 0)
        //                .Select(fp => fp.PriceAfterDiscount)
        //                .DefaultIfEmpty(double.MaxValue)
        //                .Min();
        //            return new StationInfo
        //            {
        //                Station = st,
        //                ForwardDistanceKm = km,
        //                PricePerLiter = price
        //            };
        //        })
        //        .ToList();

        //    // 2) Аннотируем обязательные остановки их километражем и сортируем по возрастанию
        //    var requiredInfos = requiredStops
        //        .Select(r =>
        //        {
        //            var si = stationInfos.FirstOrDefault(x => x.Station.Id == r.StationId);
        //            return si is null
        //                ? null
        //                : new
        //                {
        //                    Info = si,
        //                    RefillLiters = Math.Min(r.RefillLiters, tankCapacity)
        //                };
        //        })
        //        .Where(x => x != null)
        //        .OrderBy(x => x.Info.ForwardDistanceKm)
        //        .ToList()!;

        //    var result = new List<FuelStopPlan>();
        //    var usedStationIds = new HashSet<Guid>();
        //    double prevKm = 0.0;
        //    double remainingFuel = currentFuelLiters;

        //    // Вспомощный локальный метод: планирует дозаправки, чтобы достичь targetKm
        //    void PlanTill(double targetKm)
        //    {
        //        // сколько топлива потребуется от prevKm до targetKm
        //        double needed = (targetKm - prevKm) * fuelConsumptionPerKm;

        //        while (remainingFuel < needed)
        //        {
        //            double maxReachKm = prevKm + remainingFuel / fuelConsumptionPerKm;
        //            // ищем reachable-станции между prevKm и maxReachKm, ещё не использованные
        //            var candidates = stationInfos
        //                .Where(si =>
        //                    si.Station.Id is var id &&
        //                    !usedStationIds.Contains(id) &&
        //                    si.ForwardDistanceKm > prevKm &&
        //                    si.ForwardDistanceKm <= maxReachKm)
        //                .OrderBy(si => si.PricePerLiter)
        //                .ToList();
        //            if (!candidates.Any())
        //                break; // не дотянем — выходим

        //            var best = candidates.First();
        //            // проезжаем до best
        //            double dist = best.ForwardDistanceKm - prevKm;
        //            remainingFuel -= dist * fuelConsumptionPerKm;

        //            // дозаправляем (минимум 30 л или до полного бака)
        //            double toFull = tankCapacity - remainingFuel;
        //            double refill = Math.Min(Math.Max(toFull, 30.0), toFull);
        //            remainingFuel += refill;

        //            result.Add(new FuelStopPlan
        //            {
        //                Station = best.Station!,
        //                StopAtKm = best.ForwardDistanceKm,
        //                RefillLiters = refill
        //            });
        //            usedStationIds.Add(best.Station!.Id);
        //            prevKm = best.ForwardDistanceKm;

        //            // пересчитываем, сколько ещё нужно
        //            needed = (targetKm - prevKm) * fuelConsumptionPerKm;
        //        }

        //        // «проезжаем» до targetKm без дозаправки
        //        remainingFuel -= (targetKm - prevKm) * fuelConsumptionPerKm;
        //        prevKm = targetKm;
        //    }

        //    // 3) Пробегаем по всем обязательным остановкам
        //    foreach (var req in requiredInfos)
        //    {
        //        double kmReq = req.Info.ForwardDistanceKm;
        //        // сначала доехать (или дозаправиться на ходу), чтобы достичь kmReq
        //        PlanTill(kmReq);

        //        // теперь делаем обязательную дозаправку ровно req.RefillLiters
        //        double allowed = Math.Min(req.RefillLiters, tankCapacity - remainingFuel);
        //        remainingFuel += allowed;
        //        result.Add(new FuelStopPlan
        //        {
        //            Station = req.Info.Station!,
        //            StopAtKm = kmReq,
        //            RefillLiters = allowed
        //        });
        //        usedStationIds.Add(req.Info.Station!.Id);
        //        // prevKm уже == kmReq
        //    }

        //    // 4) Наконец, добираемся от последней обязательной до конца маршрута
        //    PlanTill(totalRouteDistanceKm);

        //    return result;
        //}
        // Вспомогательный метод: вычисляем «километраж» вдоль маршрута, где расположена станция
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
           double nextDistanceKm)
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
                NextDistanceKm = nextDistanceKm.ToString("F2")
            };
        }


        private FuelStationDto FuelStationToDtoNoAlgorithm(
         FuelStation station)
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

                IsAlgorithm = false
            };
        }


        // --------------------------------------------------------
        //  Вспомогательный класс: храним инфу о «станции + километре вдоль маршрута + цене»
        // --------------------------------------------------------
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


        // --------------------------------------------------------
        //  Компаратор для SortedSet<StationInfo> по цене (и по distance, если цены равны).
        // --------------------------------------------------------
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

        // --------------------------------------------------------
        //  Вспомогательный метод: перевод градусов в радианы
        // --------------------------------------------------------
        private static double DegreesToRadians(double degrees)
            => degrees * (Math.PI / 180.0);
    }

    // ---------------------------------------------
    //  Модель для плана остановки (DTO-like структура)
    // ---------------------------------------------
    public class FuelStopPlan
    {
        public FuelStation Station { get; set; } = null!;

        /// <summary>
        /// Километр вдоль маршрута, где происходит остановка.
        /// </summary>
        public double StopAtKm { get; set; }

        /// <summary>
        /// Сколько литров заливаем.
        /// </summary>
        public double RefillLiters { get; set; }

        public double CurrentFuelLiters { get; set; }
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
    }
}
