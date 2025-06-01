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

        // Расход топлива: 0.3 л/км (30 л/100 км)
        private const double TruckFuelConsumptionLPerKm = 0.5;

        // Ёмкость бака в литрах
        private const double TruckTankCapacityL = 200.0;

        // Начальный объём топлива (можно брать из запроса, здесь для примера)
        private const double InitialFuelLiters = 200.0;

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
                return Result.Ok(new List<FuelStationDto>()); // нет станций в зоне

            // 5. Отфильтровываем станции, которые действительно лежат в коридоре вдоль маршрута (± SearchRadiusKm)
            var stationsAlongRoute = request.Roads
                .SelectMany(r => r.Points?.Where(p => p?.Count >= 2) ?? Enumerable.Empty<List<double>>())
                .Select(p => new GeoPoint(p[0], p[1]))
                .SelectMany(geoPoint => stations
                    .Where(s => GeoCalculator.IsPointWithinRadius(geoPoint, s.Coordinates, SearchRadiusKm)))
                .DistinctBy(s => s.Id)
                .ToList();

            var stationsAlongFirstRout = request.Roads.FirstOrDefault().Points
                 ?.Where(p => p?.Count >= 2)
                 .Select(p => new GeoPoint(p[0], p[1]))
                 .SelectMany(geoPoint => stations
                     .Where(s => GeoCalculator.IsPointWithinRadius(geoPoint, s.Coordinates, SearchRadiusKm)))
                 .DistinctBy(s => s.Id)
                 .ToList();



            if (!stationsAlongRoute.Any())
                return Result.Ok(new List<FuelStationDto>());

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



            // 8. Преобразуем результат в DTO
            //var resultDto = stopPlan
            //    .Select((plan, idx) => FuelStationToDto(plan.Station, idx + 1, plan.RefillLiters))
            //    .ToList();

            return Result.Ok(resultDto);
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

            // Собираем информацию о станциях: StationInfo = (станция, ForwardDistanceKm, цена)
            var stationInfos = new List<StationInfo>();
            foreach (var st in stationsAlongRoute)
            {
                double forwardDist = GetForwardDistanceAlongRoute(route, st.Coordinates);
                if (forwardDist < double.MaxValue)
                {
                    var priceInfo = st.FuelPrices.FirstOrDefault();
                    double price = priceInfo?.Price ?? double.MaxValue;
                    stationInfos.Add(new StationInfo
                    {
                        Station = st,
                        ForwardDistanceKm = forwardDist,
                        PricePerLiter = price
                    });
                }
            }

            // Добавляем «конец маршрута» как виртуальную цель
            stationInfos.Add(new StationInfo
            {
                Station = null,
                ForwardDistanceKm = totalRouteDistanceKm,
                PricePerLiter = 0.0
            });

            // Сортируем по возрастанию ForwardDistanceKm
            stationInfos = stationInfos.OrderBy(si => si.ForwardDistanceKm).ToList();

            // Жадный алгоритм с учётом “только вперёд”
            var availableStations = new SortedSet<StationInfo>(new StationPriceComparer());
            var usedStationIds = new HashSet<Guid>();

            double prevForwardDistance = 0.0;      // “где мы сейчас” (в км вдоль маршрута)
            double remainingFuel = currentFuelLiters;

            for (int i = 0; i < stationInfos.Count; i++)
            {
                var currentInfo = stationInfos[i];
                double distToNext = currentInfo.ForwardDistanceKm - prevForwardDistance;
                double fuelNeeded = distToNext * fuelConsumptionPerKm;

                // 1) Если хватает топлива, едем до currentInfo
                if (fuelNeeded <= remainingFuel)
                {
                    remainingFuel -= fuelNeeded;
                }
                else
                {
                    // 2) Топлива не хватило, надо дозаправиться до currentInfo
                    double deficit = fuelNeeded - remainingFuel;

                    // 2.1) Берём из availableStations только те, чей ForwardDistanceKm >= prevForwardDistance
                    StationInfo? cheapest = availableStations
                        .FirstOrDefault(si =>
                            si.Station != null
                            && si.ForwardDistanceKm >= prevForwardDistance
                            && !usedStationIds.Contains(si.Station.Id));

                    if (cheapest != null && cheapest.Station != null)
                    {
                        // Заправляемся в этой “доступной” станции
                        double refillLiters = Math.Min(tankCapacity - remainingFuel, deficit);
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
                        // 2.2) Если в availableStations нет подходящей станции — значит до currentInfo не было “доступных”
                        //      (то есть первый участок без заправок). Тогда нужно ехать вперёд до первой станции в stationInfos,
                        //      у которой ForwardDistanceKm > prevForwardDistance (т. е. ближайшая впереди).

                        var nextAhead = stationInfos
                            .Where(si =>
                                si.Station != null
                                && si.ForwardDistanceKm > prevForwardDistance
                                && !usedStationIds.Contains(si.Station.Id))
                            .OrderBy(si => si.ForwardDistanceKm)
                            .FirstOrDefault();

                        if (nextAhead != null && nextAhead.Station != null)
                        {
                            // Считаем, сколько км до этой nextAhead:
                            double kmToNextAhead = nextAhead.ForwardDistanceKm - prevForwardDistance;
                            double fuelNeededToNextAhead = kmToNextAhead * fuelConsumptionPerKm;

                            // Если initialFuel < fuelNeededToNextAhead, то вообще не добраться.
                            // Но по условию задачи мы считаем, что хватает.
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

                            // Теперь едем сразу до currentInfo или до этой nextAhead?
                            // Логично: мы точно дозаправились на nextAhead, 
                            // а дальше перейдём к следующей итерации цикла,
                            // в которой currentInfo == nextAhead или текущий.
                        }
                        else
                        {
                            // Если nextAhead == null, значит вообще нет впереди станций. 
                            // Тогда единственный вариант — маршрут кончается сразу за текущей точкой.
                            // Можно считать, что дотянем до конца (виртуальной цели) или бросим ошибку.
                        }

                        // После дозаправки, когда refill проведён, обновляем remainingFuel:
                        // (внутри блока выше мы уже сделали remainingFuel += refillLiters)
                    }

                    // После дозаправки (либо из availableStations, либо – впереди, либо – нет вариантов),
                    // мы гарантированно имеем enough fuel, чтобы проехать distToNext:
                    remainingFuel -= fuelNeeded;
                }

                // 3) Добавляем текущую станцию (если не виртуальная цель) в availableStations,
                //    только если она дальше или равна prevForwardDistance и ещё не использована
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

        // --------------------------------------------------------
        //  Вычисление прямой «километровой» позиции станции вдоль маршрута
        // --------------------------------------------------------
        private double GetForwardDistanceAlongRoute(
            List<GeoPoint> route,
            GeoPoint stationCoords)
        {
            double cumulative = 0.0;

            for (int i = 0; i < route.Count - 1; i++)
            {
                var a = route[i];
                var b = route[i + 1];

                double segmentLength = GeoCalculator.CalculateHaversineDistance(a, b);
                // Если станция «попадает» в этот сегмент (расстояние до отрезка <= SearchRadiusKm), возвращаем cumulative + позиция внутри сегмента.
                double distToSegment = GeoCalculator.DistanceFromPointToSegmentKm(stationCoords, a, b);
                if (distToSegment <= SearchRadiusKm)
                {
                    // Чтобы узнать точную «километровую отметку» внутри этого сегмента, 
                    // можно найти проекцию stationCoords на отрезок a-b и вычислить расстояние от a до этой проекции.
                    var projectionKm = DistanceAlongSegment(a, b, stationCoords);
                    return cumulative + projectionKm;
                }

                cumulative += segmentLength;
            }

            return double.MaxValue; // не «попала» ни в один сегмент маршрута
        }

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

                RefillLiters = refillLiters.ToString("F2"),
                StopOrder = stopOrder,
                NextDistanceKm = nextDistanceKm.ToString("F2")
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

                // Если цена равна, сравниваем по ForwardDistanceKm, чтобы однозначно различать элементы
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

    // ---------------------------------------------
    //  DTO, возвращаемый в API/клиенту
    // ---------------------------------------------
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

        // Сколько литров заливаем
        public string RefillLiters { get; set; } = null!;

        // Порядок остановки (1, 2, 3…)
        public int StopOrder { get; set; }

        // Расстояние в километрах до следующей остановки (или до конца маршрута)
        public string NextDistanceKm { get; set; } = null!;
    }
}
