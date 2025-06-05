using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Domain.FuelStations;

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
        private const double TruckTankCapacityL = 200.0;

        // Начальный объём топлива: 60 галлонов
        private const double InitialFuelLiters = 60.0;

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

            // 3. Вычисляем «коридорный» bounding-box по широте/долготе, чтобы узко отфильтровать станции
            //    (400 км = приблизительно 4° широты, но мы считаем через деление на 111 км/°, чтобы преобразовать SearchRadiusKm в градусы)
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


            // 6. Считаем общую длину маршрута (в км)
            var route = request.Roads.FirstOrDefault().Points.Select(p => new GeoPoint(p[0], p[1])).ToList();
            double totalRouteDistanceKm = 0;
            for (int i = 0; i < route.Count - 1; i++)
            {
                totalRouteDistanceKm += GeoCalculator.CalculateHaversineDistance(route[i], route[i + 1]);
            }

            // 7. Запускаем алгоритм «по заправкам» с оптимизацией по цене
            var stopPlan = PlanStopsByStations(
                routePoints,
                stationsAlongFirstRout,
                totalRouteDistanceKm,
                TruckFuelConsumptionLPerKm,
                InitialFuelLiters,
                TruckTankCapacityL);


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
                    // Создаем новый объект FuelStationDto с обновленными полями
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
                // Возвращаем копию станции без изменений, если совпадений нет
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

        // --------------------------------------------------------
        //  Шаг 5: Отфильтровать станции вдоль маршрута, используя «коридорный» подход
        // --------------------------------------------------------
        private List<FuelStation> GetStationsAlongRoute(
            List<GeoPoint> route,
            List<FuelStation> allStations,
            double corridorRadiusKm)
        {
            var result = new HashSet<FuelStation>();

            for (int i = 0; i < route.Count - 1; i++)
            {
                var a = route[i];
                var b = route[i + 1];

                foreach (var station in allStations)
                {
                    double distToSegment = GeoCalculator.DistanceFromPointToSegmentKm(
                        station.Coordinates, a, b);

                    if (distToSegment <= corridorRadiusKm)
                    {
                        result.Add(station);
                    }
                }
            }

            return result.ToList();
        }

        // --------------------------------------------------------
        //  Шаг 7: Основной метод, который планирует остановки «по заправкам»
        // --------------------------------------------------------
        private List<FuelStopPlan> PlanStopsByStations(
        List<GeoPoint> route,
        List<FuelStation> stationsAlongRoute,
        double totalRouteDistanceKm,
        double fuelConsumptionPerKm,
        double currentFuelLiters,
        double tankCapacity)
        {
            var result = new List<FuelStopPlan>();

            // 1. Собираем информацию о станциях: StationInfo = (станция, ForwardDistanceKm, цена после скидки)
            var stationInfos = new List<StationInfo>();
            foreach (var st in stationsAlongRoute)
            {
                double forwardDist = GetForwardDistanceAlongRoute(route, st.Coordinates);
                if (forwardDist < double.MaxValue)
                {
                    // Выбираем самую низкую цену после скидки среди всех FuelPrices
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

            // 2. Добавляем «конец маршрута» как виртуальную цель
            stationInfos.Add(new StationInfo
            {
                Station = null,
                ForwardDistanceKm = totalRouteDistanceKm,
                PricePerLiter = 0.0
            });

            // 3. Сортируем по возрастанию ForwardDistanceKm
            stationInfos = stationInfos.OrderBy(si => si.ForwardDistanceKm).ToList();

            // 4. Жадный алгоритм: выбираем, где дешевле, и в этих местах заправляем максимально
            var availableStations = new SortedSet<StationInfo>(new StationPriceComparer());
            var usedStationIds = new HashSet<Guid>();

            double prevForwardDistance = 0.0;      // “где мы сейчас” (в км вдоль маршрута)
            double remainingFuel = currentFuelLiters;

            for (int i = 0; i < stationInfos.Count; i++)
            {
                var currentInfo = stationInfos[i];
                double distToNext = currentInfo.ForwardDistanceKm - prevForwardDistance;
                double fuelNeeded = distToNext * fuelConsumptionPerKm;

                // 1) Если хватает топлива, едем до currentInfo без дозаправки
                if (fuelNeeded <= remainingFuel)
                {
                    remainingFuel -= fuelNeeded;
                }
                else
                {
                    // 2) Топлива не хватило, надо дозаправиться до текущей точки (currentInfo)
                    double deficit = fuelNeeded - remainingFuel;

                    // 2.1) Из уже «доступных» станций (availableStations) выбираем ту, у которой самая низкая цена
                    StationInfo? cheapest = availableStations
                        .FirstOrDefault(si =>
                            si.Station != null
                            && si.ForwardDistanceKm >= prevForwardDistance
                            && !usedStationIds.Contains(si.Station.Id));

                    if (cheapest != null && cheapest.Station != null)
                    {
                        // Заправляемся в этой «доступной» (дешевой) станции по максимуму
                        double refillLiters = tankCapacity - remainingFuel;
                        result.Add(new FuelStopPlan
                        {
                            Station = cheapest.Station,
                            RefillLiters = refillLiters,
                            StopAtKm = cheapest.ForwardDistanceKm
                        });

                        usedStationIds.Add(cheapest.Station.Id);
                        availableStations.Remove(cheapest);

                        remainingFuel += refillLiters;
                    }
                    else
                    {
                        // 2.2) Если в availableStations нет подходящей — ищем ближайшую впереди
                        var nextAhead = stationInfos
                            .Where(si =>
                                si.Station != null
                                && si.ForwardDistanceKm > prevForwardDistance
                                && !usedStationIds.Contains(si.Station.Id))
                            .OrderBy(si => si.ForwardDistanceKm)
                            .FirstOrDefault();

                        if (nextAhead != null && nextAhead.Station != null)
                        {
                            // Считаем, сколько топлива нужно до nextAhead
                            double kmToNextAhead = nextAhead.ForwardDistanceKm - prevForwardDistance;
                            double fuelNeededToNextAhead = kmToNextAhead * fuelConsumptionPerKm;

                            // Заправляем столько, чтобы в любом случае добраться до nextAhead,
                            // но при этом максимально заполняем бак
                            double refillLiters = Math.Min(tankCapacity - remainingFuel, fuelNeededToNextAhead - remainingFuel);
                            if (refillLiters < 0) refillLiters = 0;

                            result.Add(new FuelStopPlan
                            {
                                Station = nextAhead.Station,
                                RefillLiters = refillLiters,
                                StopAtKm = nextAhead.ForwardDistanceKm
                            });

                            usedStationIds.Add(nextAhead.Station.Id);
                            availableStations.Remove(nextAhead);

                            remainingFuel += refillLiters;
                        }
                        else
                        {
                            // Если впереди нет станций, можно считать, что доберёмся до конца маршрута (виртуальной цели)
                            // Либо предусмотреть логику ошибки, если топлива всё равно не хватает
                        }
                    }

                    // После дозаправки eдем до currentInfo
                    remainingFuel -= fuelNeeded;
                }

                // 3) Добавляем текущую станцию (если не виртуальная цель) в availableStations,
                //    если она ещё не использована и находится дальше prevForwardDistance
                if (currentInfo.Station != null
                    && currentInfo.ForwardDistanceKm >= prevForwardDistance
                    && !usedStationIds.Contains(currentInfo.Station.Id))
                {
                    availableStations.Add(currentInfo);
                }

                prevForwardDistance = currentInfo.ForwardDistanceKm;
            }

            return result;
        }


        // Вспомогательный метод: вычисляем «километровую позицию» вдоль сегмента AB для станции
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
                    var projectionKm = GeoCalculator.DistanceAlongSegment(a, b, stationCoords);
                    return cumulative + projectionKm;
                }
                cumulative += segmentLength;
            }
            return double.MaxValue;
        }



        //private double GetForwardDistanceAlongRoute(
        //    List<GeoPoint> route,
        //    GeoPoint stationCoords)
        //{
        //    double cumulative = 0.0;

        //    for (int i = 0; i < route.Count - 1; i++)
        //    {
        //        var a = route[i];
        //        var b = route[i + 1];

        //        double segmentLength = GeoCalculator.CalculateHaversineDistance(a, b);
        //        // Если станция «попадает» в этот сегмент (расстояние до отрезка <= SearchRadiusKm), возвращаем cumulative + позиция внутри сегмента.
        //        double distToSegment = GeoCalculator.DistanceFromPointToSegmentKm(stationCoords, a, b);
        //        if (distToSegment <= SearchRadiusKm)
        //        {
        //            // Чтобы узнать точную «километровую отметку» внутри этого сегмента, 
        //            // можно найти проекцию stationCoords на отрезок a-b и вычислить расстояние от a до этой проекции.
        //            var projectionKm = DistanceAlongSegment(a, b, stationCoords);
        //            return cumulative + projectionKm;
        //        }

        //        cumulative += segmentLength;
        //    }

        //    return double.MaxValue; // не «попала» ни в один сегмент маршрута
        //}

        // --------------------------------------------------------
        //  Помощь: найти, на каком километре этого сегмента (a→b) лежит станция
        // --------------------------------------------------------
        private double DistanceAlongSegment(GeoPoint a, GeoPoint b, GeoPoint p)
        {
            // Переводим точки в векторы (в градусах)
            double lat1 = a.Latitude, lon1 = a.Longitude;
            double lat2 = b.Latitude, lon2 = b.Longitude;
            double lat3 = p.Latitude, lon3 = p.Longitude;

            // В декартовых проекциях (эта приближённая формула сохраняет направление вдоль сегмента)
            double dx = lat2 - lat1;
            double dy = lon2 - lon1;

            if (dx == 0 && dy == 0)
                return 0.0;

            // Параметр t проекции p на отрезок ab (в долях от 0 до 1)
            double t = ((lat3 - lat1) * dx + (lon3 - lon1) * dy) / (dx * dx + dy * dy);
            t = Math.Max(0.0, Math.Min(1.0, t));

            // Координаты «точки на отрезке»
            double projLat = lat1 + t * dx;
            double projLon = lon1 + t * dy;

            // И реально считаем расстояние от a до проекции
            return GeoCalculator.CalculateHaversineDistance(a, new GeoPoint(projLat, projLon));
        }

        // --------------------------------------------------------
        //  Fallback: Находим «ближайшую станцию» ещё дальше по маршруту (в пределах коридора)
        // --------------------------------------------------------
        private StationInfo? FindNextClosestStation(
            List<GeoPoint> route,
            int currentIndex,
            List<FuelStation> stationsAlongRoute,
            HashSet<Guid> usedStationIds)
        {
            // Проходим по всем будущим точкам маршрута и проверяем, есть ли непосещённая станция в коридоре.
            // Берём самую «раннюю» (по пути) станцию.
            for (int i = currentIndex + 1; i < route.Count; i++)
            {
                var waypoint = route[i];

                var candidates = stationsAlongRoute
                    .Where(s => !usedStationIds.Contains(s.Id) &&
                        GeoCalculator.DistanceFromPointToSegmentKm(waypoint, waypoint, s.Coordinates) <= SearchRadiusKm)
                    .Select(s =>
                    {
                        double fd = GetForwardDistanceAlongRoute(route, s.Coordinates);
                        var price = s.FuelPrices.FirstOrDefault()?.Price ?? double.MaxValue;
                        return new StationInfo
                        {
                            Station = s,
                            ForwardDistanceKm = fd,
                            PricePerLiter = price
                        };
                    })
                    .OrderBy(si => si.PricePerLiter)
                    .ToList();

                if (candidates.Any())
                    return candidates.First();
            }

            return null;
        }

        // --------------------------------------------------------
        //  Конвертация в DTO
        // --------------------------------------------------------
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
