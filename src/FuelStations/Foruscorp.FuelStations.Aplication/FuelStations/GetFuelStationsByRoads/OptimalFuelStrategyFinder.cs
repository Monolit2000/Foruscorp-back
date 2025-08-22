using System;
using System.Collections.Generic;
using System.Linq;
using Foruscorp.FuelStations.Domain.FuelStations;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    /// <summary>
    /// Находит оптимальную стратегию заправок грузовика с минимальной стоимостью
    /// </summary>
    public class OptimalFuelStrategyFinder
    {
        private const int MaxStationsToConsider = 16; // Ограничение на количество заправок для оптимизации

        /// <summary>
        /// Вычисляет оптимальные остановки для заправки
        /// </summary>
        /// <param name="stations">Список заправок</param>
        /// <param name="totalRouteDistanceKm">Общая длина маршрута</param>
        /// <param name="fuelCapacity">Объем бака в литрах</param>
        /// <param name="consumption">Расход топлива в л/100км</param>
        /// <param name="finishFuel">Требуемый остаток топлива на финише</param>
        /// <returns>Оптимальный план заправок</returns>
        public static List<FuelStopPlan> CalculateOptimalStops(
            List<StationInfo> stations,
            double totalRouteDistanceKm,
            double fuelCapacity,
            double consumption,
            double finishFuel)
        {
            if (stations == null || !stations.Any())
                return new List<FuelStopPlan>();

            // Фильтруем заправки с валидными ценами и сортируем по расстоянию
            var validStations = stations
                .Where(s => s.PricePerLiter > 0 && s.ForwardDistanceKm <= totalRouteDistanceKm)
                .OrderBy(s => s.ForwardDistanceKm)
                .ToList();

            if (!validStations.Any())
                return new List<FuelStopPlan>();

            // Оптимизация: выбираем только K самых дешевых заправок
            var optimizedStations = SelectOptimalStations(validStations, fuelCapacity, consumption);

            // Добавляем фиктивную конечную точку
            var allStations = new List<StationInfo>(optimizedStations)
            {
                new StationInfo
                {
                    Station = null!, // Фиктивная станция
                    ForwardDistanceKm = totalRouteDistanceKm,
                    PricePerLiter = double.MaxValue // Очень дорогая, чтобы избежать заправки
                }
            };

            // Используем динамическое программирование для поиска оптимального маршрута
            var result = FindOptimalRouteWithDP(allStations, fuelCapacity, consumption, finishFuel);

            // Преобразуем результат в FuelStopPlan, исключая фиктивную конечную точку
            return result.Where(plan => plan.Station != null).ToList();
        }

        /// <summary>
        /// Выбирает оптимальные заправки для рассмотрения (K самых дешевых в пределах досягаемости)
        /// </summary>
        private static List<StationInfo> SelectOptimalStations(List<StationInfo> stations, double fuelCapacity, double consumption)
        {
            var maxRange = (fuelCapacity / consumption); // * 100;  Максимальная дальность в км
            var reachableStations = new List<StationInfo>();

            // Начинаем с первой заправки
            reachableStations.Add(stations[0]);

            for (int i = 1; i < stations.Count; i++)
            {
                var currentStation = stations[i];
                var prevStation = reachableStations.Last();
                var distance = currentStation.ForwardDistanceKm - prevStation.ForwardDistanceKm;

                // Проверяем, достижима ли станция
                if (currentStation.ForwardDistanceKm <= maxRange)
                {
                    reachableStations.Add(currentStation);
                }
            }

            // Если заправок слишком много, выбираем K самых дешевых
            if (reachableStations.Count > MaxStationsToConsider)
            {
                return reachableStations
                    .OrderBy(s => s.PricePerLiter)
                    .Take(MaxStationsToConsider)
                    .OrderBy(s => s.ForwardDistanceKm)
                    .ToList();
            }

            return reachableStations;
        }

        /// <summary>
        /// Находит оптимальный маршрут с помощью динамического программирования
        /// </summary>
        private static List<FuelStopPlan> FindOptimalRouteWithDP(
            List<StationInfo> stations,
            double fuelCapacity,
            double consumption,
            double finishFuel)
        {
            var n = stations.Count;
            if (n == 0) return new List<FuelStopPlan>();

            // Дискретизируем уровень топлива для DP
            const int fuelLevels = 101; // 0%, 1%, 2%, ..., 100%
            var fuelStep = fuelCapacity / (fuelLevels - 1);

            // dp[i, fuel] = минимальная стоимость достичь станции i с уровнем топлива fuel
            var dp = new double[n, fuelLevels];
            var prev = new (int station, int fuel)[n, fuelLevels];

            // Инициализируем все значения как бесконечность
            for (int i = 0; i < n; i++)
            {
                for (int f = 0; f < fuelLevels; f++)
                {
                    dp[i, f] = double.MaxValue;
                    prev[i, f] = (-1, -1);
                }
            }

            // Базовый случай: начинаем с первой станции с полным баком
            dp[0, fuelLevels - 1] = 0.0;

            // Заполняем таблицу DP
            for (int i = 0; i < n - 1; i++)
            {
                for (int fuel = 0; fuel < fuelLevels; fuel++)
                {
                    if (dp[i, fuel] == double.MaxValue) continue;

                    var currentFuel = fuel * fuelStep;
                    var currentStation = stations[i];

                    // Перебираем все возможные следующие станции
                    for (int j = i + 1; j < n; j++)
                    {
                        var nextStation = stations[j];
                        var distance = nextStation.ForwardDistanceKm - currentStation.ForwardDistanceKm;
                        var fuelNeeded = (distance / 100.0) * consumption;

                        // Проверяем, можем ли доехать до следующей станции
                        if (fuelNeeded > currentFuel) continue;

                        var remainingFuel = currentFuel - fuelNeeded;
                        var remainingFuelLevel = (int)(remainingFuel / fuelStep);

                        // Перебираем все возможные количества топлива для заправки
                        for (int refillLevel = remainingFuelLevel; refillLevel < fuelLevels; refillLevel++)
                        {
                            var refillAmount = (refillLevel - remainingFuelLevel) * fuelStep;
                            var refillCost = refillAmount * currentStation.PricePerLiter;
                            var totalCost = dp[i, fuel] + refillCost;

                            if (totalCost < dp[j, refillLevel])
                            {
                                dp[j, refillLevel] = totalCost;
                                prev[j, refillLevel] = (i, fuel);
                            }
                        }
                    }
                }
            }

            // Находим оптимальное решение
            var minCost = double.MaxValue;
            var bestStation = -1;
            var bestFuel = -1;

            for (int fuel = 0; fuel < fuelLevels; fuel++)
            {
                var finalFuel = fuel * fuelStep;
                if (finalFuel >= finishFuel && dp[n - 1, fuel] < minCost)
                {
                    minCost = dp[n - 1, fuel];
                    bestStation = n - 1;
                    bestFuel = fuel;
                }
            }

            if (minCost == double.MaxValue)
                return new List<FuelStopPlan>();

            // Восстанавливаем путь
            return ReconstructPath(stations, prev, bestStation, bestFuel, fuelStep, consumption);
        }

        /// <summary>
        /// Восстанавливает оптимальный путь из таблицы DP
        /// </summary>
        private static List<FuelStopPlan> ReconstructPath(
            List<StationInfo> stations,
            (int station, int fuel)[,] prev,
            int endStation,
            int endFuel,
            double fuelStep,
            double consumption)
        {
            var path = new List<FuelStopPlan>();
            var currentStation = endStation;
            var currentFuel = endFuel;

            while (currentStation != -1)
            {
                var station = stations[currentStation];
                var fuelLevel = currentFuel * fuelStep;

                // Вычисляем количество заправленного топлива
                double refillAmount = 0;
                if (path.Count > 0)
                {
                    var prevStop = path[0]; // Последняя добавленная остановка
                    var distance = station.ForwardDistanceKm - prevStop.StopAtKm;
                    var fuelUsed = (distance / 100.0) * consumption;
                    var fuelBeforeRefill = prevStop.CurrentFuelLiters - fuelUsed;
                    refillAmount = fuelLevel - fuelBeforeRefill;
                }

                var stopPlan = new FuelStopPlan
                {
                    Station = station.Station,
                    StopAtKm = station.ForwardDistanceKm,
                    RefillLiters = Math.Max(0, refillAmount),
                    CurrentFuelLiters = fuelLevel
                };

                path.Insert(0, stopPlan);

                var prevState = prev[currentStation, currentFuel];
                currentStation = prevState.station;
                currentFuel = prevState.fuel;
            }

            return path;
        }

        /// <summary>
        /// Пример использования метода
        /// </summary>
        public static void ExampleUsage()
        {
            // Создаем тестовые заправки
            var stations = new List<StationInfo>
            {
                new StationInfo
                {
                    Station = CreateTestStation("Заправка 1", 0),
                    ForwardDistanceKm = 0,
                    PricePerLiter = 2.5
                },
                new StationInfo
                {
                    Station = CreateTestStation("Заправка 2", 100),
                    ForwardDistanceKm = 100,
                    PricePerLiter = 2.3
                },
                new StationInfo
                {
                    Station = CreateTestStation("Заправка 3", 200),
                    ForwardDistanceKm = 200,
                    PricePerLiter = 2.7
                },
                new StationInfo
                {
                    Station = CreateTestStation("Заправка 4", 300),
                    ForwardDistanceKm = 300,
                    PricePerLiter = 2.1
                },
                new StationInfo
                {
                    Station = CreateTestStation("Заправка 5", 400),
                    ForwardDistanceKm = 400,
                    PricePerLiter = 2.4
                }
            };

            // Параметры маршрута
            double totalRouteDistanceKm = 500;
            double fuelCapacity = 200; // литров
            double consumption = 8.9; // л/100км
            double finishFuel = 40; // литров

            // Вычисляем оптимальный маршрут
            var optimalStops = CalculateOptimalStops(
                stations,
                totalRouteDistanceKm,
                fuelCapacity,
                consumption,
                finishFuel);

            // Выводим результат
            Console.WriteLine("Оптимальный маршрут заправок:");
            Console.WriteLine("================================");

            double totalCost = 0;
            for (int i = 0; i < optimalStops.Count; i++)
            {
                var stop = optimalStops[i];
                var cost = stop.RefillLiters * stations.First(s => s.Station.Id == stop.Station.Id).PricePerLiter;
                totalCost += cost;

                Console.WriteLine($"Остановка {i + 1}:");
                Console.WriteLine($"  Станция: {stop.Station.ProviderName}");
                Console.WriteLine($"  Расстояние: {stop.StopAtKm:F1} км");
                Console.WriteLine($"  Заправлено: {stop.RefillLiters:F1} л");
                Console.WriteLine($"  Остаток в баке: {stop.CurrentFuelLiters:F1} л");
                Console.WriteLine($"  Стоимость: {cost:F2} руб.");
                Console.WriteLine();
            }

            Console.WriteLine($"Общая стоимость маршрута: {totalCost:F2} руб.");
        }

        /// <summary>
        /// Создает тестовую заправку
        /// </summary>
        private static FuelStation CreateTestStation(string name, double distance)
        {
            return FuelStation.CreateNew(
                $"Адрес {name}",
                name,
                new GeoPoint(0, 0),
                new List<FuelPrice> { new FuelPrice(FuelType.Diesel, 2.5) }
            );
        }
    }
}
