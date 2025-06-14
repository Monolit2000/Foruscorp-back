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

            // ---------------------------------------
            // Шаг 0. Собираем stationInfos (без виртуальной цели)
            // ---------------------------------------
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

            // Добавляем виртуальную «цель» (конец маршрута), чтобы понимать, что до следующей реальной заправки не доехать
            var endInfo = new StationInfo
            {
                Station = null,
                ForwardDistanceKm = totalRouteDistanceKm,
                PricePerLiter = 0.0
            };
            stationInfos.Add(endInfo);

            // Сортируем по возрастанию ForwardDistanceKm (необязательно, но удобно для поиска ближайших)
            stationInfos = stationInfos.OrderBy(si => si.ForwardDistanceKm).ToList();

            // ---------------------------------------
            // Шаг 1. Задаём начальные условия
            // ---------------------------------------
            double prevForwardDistance = 0.0;         // Мы стартуем от «0 км»
            double remainingFuel = currentFuelLiters; // Топливо, с которым выехали
            var usedStationIds = new HashSet<Guid>(); // Станции, где мы уже заправлялись

            // ---------------------------------------
            // Шаг 2. Основной цикл: выбираем по очереди самую дешёвую из достижимых станций
            // ---------------------------------------
            while (true)
            {
                // (а) Если уже можем без дозаправки доехать до конца маршрута => выходим
                double distanceLeftToEnd = totalRouteDistanceKm - prevForwardDistance;
                double fuelNeededToFinish = distanceLeftToEnd * fuelConsumptionPerKm;
                if (fuelNeededToFinish <= remainingFuel)
                {
                    // Дожали до финиша на текущем топливе, больше остановок нет
                    break;
                }

                // (б) Считаем, насколько далеко можем доехать до следующей заправки без дозаправки
                double maxReachableDistance = prevForwardDistance + (remainingFuel / fuelConsumptionPerKm);

                // (в) Собираем все станции, которые:
                //      • впереди нас (ForwardDistanceKm > prevForwardDistance)
                //      • не дальше, чем maxReachableDistance
                //      • и ещё не заправляемся (usedStationIds)

                double MinStopDistanceKm = 1000.0;

                var reachableStations = stationInfos
                    .Where(si =>

                        (!result.Any() && 
                        si.Station != null &&
                        si.ForwardDistanceKm > prevForwardDistance &&
                        si.ForwardDistanceKm <= maxReachableDistance &&
                        !usedStationIds.Contains(si.Station.Id)) ||

                        (si.Station != null &&
                        si.ForwardDistanceKm > prevForwardDistance &&
                        si.ForwardDistanceKm <= maxReachableDistance &&

                        si.ForwardDistanceKm - prevForwardDistance >= MinStopDistanceKm &&

                        !usedStationIds.Contains(si.Station.Id)))
                    .ToList();

                // (г) Если вовсе нет ни одной станции, до которой можно доехать на оставшемся топливе,
                //     значит мы не дотянем ни до конца маршрута, ни до ближайшей заправки (обычный сценарий: "недостаточно топлива").
                //     Можно просто прервать цикла — дальнейших остановок нет, но маршрут не завершится полностью.
                if (!reachableStations.Any())
                {
                    break;
                }

                // (д) Из reachableStations выбираем самую дешёвую (минимальная PricePerLiter)
                var chosenInfo = reachableStations
                    //.Where(si => !result.Any() || si.ForwardDistanceKm - prevForwardDistance >= MinStopDistanceKm)
                    .OrderBy(si => si.PricePerLiter)
                    .First();

                // (е) Считаем, сколько топлива потратим, чтобы доехать до chosenInfo
                double distanceToChosen = chosenInfo.ForwardDistanceKm - prevForwardDistance;
                double fuelNeededToChosen = distanceToChosen * fuelConsumptionPerKm;
                remainingFuel -= fuelNeededToChosen; // доехали до этой станции

                // (ж) Заправляем полный бак на этой станции
                double neededToFull = tankCapacity - remainingFuel;
                //double refillAmount = Math.Max(neededToFull, 30.0);
                double refillAmount = Math.Min(Math.Max(neededToFull, 30.0), tankCapacity - remainingFuel);
                result.Add(new FuelStopPlan
                {
                    Station = chosenInfo.Station!,
                    StopAtKm = chosenInfo.ForwardDistanceKm,
                    RefillLiters = refillAmount
                });

                // (з) Отмечаем станцию как «использованную» и обновляем параметры
                usedStationIds.Add(chosenInfo.Station!.Id);
                remainingFuel = tankCapacity;
                prevForwardDistance = chosenInfo.ForwardDistanceKm;

                // (и) Переходим к следующей итерации:
                //     либо дозаправляемся снова, либо доезжаем до финиша, либо выбираем следующую станцию и т.д.
            }

            return result;
        }

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
