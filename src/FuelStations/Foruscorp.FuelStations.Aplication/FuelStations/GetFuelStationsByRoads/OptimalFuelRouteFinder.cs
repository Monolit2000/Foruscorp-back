using System;
using System.Collections.Generic;
using System.Linq;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class OptimalFuelRouteFinder
    {
        /// <summary>
        /// Находит оптимальную цепочку заправок для минимизации стоимости топлива
        /// </summary>
        /// <param name="stationInfos">Список информации о заправках с расстояниями и ценами</param>
        /// <param name="fuelCapacity">Объем бака в литрах</param>
        /// <param name="consumption">Расход топлива в л/км</param>
        /// <param name="maxStations">Максимальное количество заправок для оптимизации (по умолчанию 16)</param>
        /// <returns>Минимальная стоимость топлива для маршрута</returns>
        public static double FindOptimalRoute(List<StationInfo> stationInfos, double fuelCapacity, double consumption, int maxStations = 16)
        {
            if (stationInfos == null || !stationInfos.Any())
                return 0.0;

            // Создаем упрощенные объекты для алгоритма
            var simplifiedStations = stationInfos.Select((s, index) => new SimplifiedStation
            {
                Id = index,
                Distance = s.ForwardDistanceKm,
                Price = s.PricePerLiter,
                StationInfo = s
            }).ToList();

            // Выбираем K самых выгодных заправок для оптимизации
            var selectedStations = SelectOptimalStations(simplifiedStations, maxStations);
            
            if (!selectedStations.Any())
                return 0.0;

            // Вычисляем максимальную дальность на полном баке
            var maxRange = fuelCapacity / consumption;

            // Запускаем битмасковое DP
            return FindOptimalRouteWithBitmaskDP(selectedStations, fuelCapacity, consumption, maxRange);
        }

        /// <summary>
        /// Выбирает оптимальные заправки для алгоритма (K самых дешевых)
        /// </summary>
        private static List<SimplifiedStation> SelectOptimalStations(List<SimplifiedStation> stations, int maxStations)
        {
            // Сортируем по цене и выбираем K самых дешевых
            return stations
                .Where(s => s.Price > 0 && s.Price < double.MaxValue) // Исключаем недействительные цены
                .OrderBy(s => s.Price)
                .Take(maxStations)
                .OrderBy(s => s.Distance) // Сортируем по расстоянию для корректной работы DP
                .ToList();
        }

        /// <summary>
        /// Реализует битмасковое динамическое программирование для поиска оптимального маршрута
        /// </summary>
        private static double FindOptimalRouteWithBitmaskDP(List<SimplifiedStation> stations, double fuelCapacity, double consumption, double maxRange)
        {
            var n = stations.Count;
            if (n == 0) return 0.0;

            // dp[mask][i] = минимальная стоимость маршрута, если мы закончили на заправке i, посетив подмножество mask
            var dp = new double[1 << n, n];
            
            // Инициализируем все значения как бесконечность
            for (int mask = 0; mask < (1 << n); mask++)
            {
                for (int i = 0; i < n; i++)
                {
                    dp[mask, i] = double.MaxValue;
                }
            }

            // Базовый случай: начинаем с первой заправки
            dp[1, 0] = 0.0; // Стоимость 0, так как мы начинаем с полным баком

            // Перебираем все возможные подмножества заправок
            for (int mask = 1; mask < (1 << n); mask++)
            {
                for (int current = 0; current < n; current++)
                {
                    // Проверяем, что текущая заправка включена в маску
                    if ((mask & (1 << current)) == 0) continue;

                    // Если стоимость еще не вычислена, пропускаем
                    if (dp[mask, current] == double.MaxValue) continue;

                    // Перебираем все возможные следующие заправки
                    for (int next = 0; next < n; next++)
                    {
                        // Проверяем, что следующая заправка еще не посещена
                        if ((mask & (1 << next)) != 0) continue;

                        // Проверяем, достижима ли следующая заправка
                        var distance = stations[next].Distance - stations[current].Distance;
                        if (distance <= 0 || distance > maxRange) continue;

                        // Рассчитываем стоимость топлива для перехода
                        var fuelNeeded = distance * consumption;
                        var fuelCost = fuelNeeded * stations[current].Price;

                        // Обновляем DP
                        var newMask = mask | (1 << next);
                        var newCost = dp[mask, current] + fuelCost;
                        
                        if (newCost < dp[newMask, next])
                        {
                            dp[newMask, next] = newCost;
                        }
                    }
                }
            }

            // Находим минимальную стоимость среди всех возможных конечных состояний
            var minCost = double.MaxValue;
            for (int mask = 1; mask < (1 << n); mask++)
            {
                for (int i = 0; i < n; i++)
                {
                    if (dp[mask, i] < double.MaxValue)
                    {
                        minCost = Math.Min(minCost, dp[mask, i]);
                    }
                }
            }

            return minCost == double.MaxValue ? 0.0 : minCost;
        }

        /// <summary>
        /// Находит оптимальный маршрут и возвращает последовательность заправок
        /// </summary>
        public static OptimalRouteResult FindOptimalRouteWithPath(List<StationInfo> stationInfos, double fuelCapacity, double consumption, int maxStations = 16)
        {
            if (stationInfos == null || !stationInfos.Any())
                return new OptimalRouteResult { TotalCost = 0.0, Route = new List<StationInfo>() };

            // Создаем упрощенные объекты для алгоритма
            var simplifiedStations = stationInfos.Select((s, index) => new SimplifiedStation
            {
                Id = index,
                Distance = s.ForwardDistanceKm,
                Price = s.PricePerLiter,
                StationInfo = s
            }).ToList();

            // Выбираем K самых выгодных заправок для оптимизации
            var selectedStations = SelectOptimalStations(simplifiedStations, maxStations);
            
            if (!selectedStations.Any())
                return new OptimalRouteResult { TotalCost = 0.0, Route = new List<StationInfo>() };

            // Вычисляем максимальную дальность на полном баке
            var maxRange = fuelCapacity / consumption;

            // Запускаем битмасковое DP с восстановлением пути
            return FindOptimalRouteWithPathDP(selectedStations, fuelCapacity, consumption, maxRange);
        }

        /// <summary>
        /// Реализует битмасковое DP с восстановлением оптимального пути
        /// </summary>
        private static OptimalRouteResult FindOptimalRouteWithPathDP(List<SimplifiedStation> stations, double fuelCapacity, double consumption, double maxRange)
        {
            var n = stations.Count;
            if (n == 0) return new OptimalRouteResult { TotalCost = 0.0, Route = new List<StationInfo>() };

            // dp[mask][i] = минимальная стоимость маршрута, если мы закончили на заправке i, посетив подмножество mask
            var dp = new double[1 << n, n];
            var prev = new int[1 << n, n]; // Для восстановления пути
            
            // Инициализируем все значения как бесконечность
            for (int mask = 0; mask < (1 << n); mask++)
            {
                for (int i = 0; i < n; i++)
                {
                    dp[mask, i] = double.MaxValue;
                    prev[mask, i] = -1;
                }
            }

            // Базовый случай: начинаем с первой заправки
            dp[1, 0] = 0.0;

            // Перебираем все возможные подмножества заправок
            for (int mask = 1; mask < (1 << n); mask++)
            {
                for (int current = 0; current < n; current++)
                {
                    // Проверяем, что текущая заправка включена в маску
                    if ((mask & (1 << current)) == 0) continue;

                    // Если стоимость еще не вычислена, пропускаем
                    if (dp[mask, current] == double.MaxValue) continue;

                    // Перебираем все возможные следующие заправки
                    for (int next = 0; next < n; next++)
                    {
                        // Проверяем, что следующая заправка еще не посещена
                        if ((mask & (1 << next)) != 0) continue;

                        // Проверяем, достижима ли следующая заправка
                        var distance = stations[next].Distance - stations[current].Distance;
                        if (distance <= 0 || distance > maxRange) continue;

                        // Рассчитываем стоимость топлива для перехода
                        var fuelNeeded = distance * consumption;
                        var fuelCost = fuelNeeded * stations[current].Price;

                        // Обновляем DP
                        var newMask = mask | (1 << next);
                        var newCost = dp[mask, current] + fuelCost;
                        
                        if (newCost < dp[newMask, next])
                        {
                            dp[newMask, next] = newCost;
                            prev[newMask, next] = current;
                        }
                    }
                }
            }

            // Находим минимальную стоимость и соответствующую конечную заправку
            var minCost = double.MaxValue;
            var bestMask = 0;
            var bestEnd = 0;

            for (int mask = 1; mask < (1 << n); mask++)
            {
                for (int i = 0; i < n; i++)
                {
                    if (dp[mask, i] < minCost)
                    {
                        minCost = dp[mask, i];
                        bestMask = mask;
                        bestEnd = i;
                    }
                }
            }

            if (minCost == double.MaxValue)
                return new OptimalRouteResult { TotalCost = 0.0, Route = new List<StationInfo>() };

            // Восстанавливаем путь
            var route = new List<StationInfo>();
            var currentMask = bestMask;
            var currentStation = bestEnd;

            while (currentStation != -1)
            {
                route.Insert(0, stations[currentStation].StationInfo);
                var prevStation = prev[currentMask, currentStation];
                currentMask &= ~(1 << currentStation);
                currentStation = prevStation;
            }

            return new OptimalRouteResult
            {
                TotalCost = minCost,
                Route = route
            };
        }
    }

    /// <summary>
    /// Упрощенная модель заправки для алгоритма
    /// </summary>
    public class SimplifiedStation
    {
        public int Id { get; set; }
        public double Distance { get; set; }
        public double Price { get; set; }
        public StationInfo StationInfo { get; set; } = null!;
    }

    /// <summary>
    /// Результат поиска оптимального маршрута
    /// </summary>
    public class OptimalRouteResult
    {
        public double TotalCost { get; set; }
        public List<StationInfo> Route { get; set; } = new List<StationInfo>();
    }
}
