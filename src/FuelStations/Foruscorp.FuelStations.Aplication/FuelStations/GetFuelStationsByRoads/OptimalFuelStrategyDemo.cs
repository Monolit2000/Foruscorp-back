using System;
using System.Collections.Generic;
using System.Linq;
using Foruscorp.FuelStations.Domain.FuelStations;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    /// <summary>
    /// Демонстрация работы алгоритма оптимальной стратегии заправок
    /// </summary>
    public class OptimalFuelStrategyDemo
    {
        /// <summary>
        /// Запускает демонстрацию алгоритма
        /// </summary>
        public static void RunDemo()
        {
            Console.WriteLine("=== ДЕМОНСТРАЦИЯ АЛГОРИТМА ОПТИМАЛЬНОЙ СТРАТЕГИИ ЗАПРАВОК ===\n");

            // Тест 1: Простой случай
            TestSimpleCase();

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Тест 2: Сложный случай с множеством заправок
            TestComplexCase();

            Console.WriteLine("\n" + new string('=', 60) + "\n");

            // Тест 3: Случай с ограниченным баком
            TestLimitedTankCase();
        }

        /// <summary>
        /// Тест 1: Простой случай с 5 заправками
        /// </summary>
        private static void TestSimpleCase()
        {
            Console.WriteLine("ТЕСТ 1: Простой случай (5 заправок)");
            Console.WriteLine("----------------------------------------");

            var stations = CreateSimpleTestStations();
            var parameters = new TestParameters
            {
                TotalRouteDistanceKm = 500,
                FuelCapacity = 200,
                Consumption = 8.9,
                FinishFuel = 40
            };

            RunTest(stations, parameters);
        }

        /// <summary>
        /// Тест 2: Сложный случай с множеством заправок
        /// </summary>
        private static void TestComplexCase()
        {
            Console.WriteLine("ТЕСТ 2: Сложный случай (множество заправок)");
            Console.WriteLine("-----------------------------------------------");

            var stations = CreateComplexTestStations();
            var parameters = new TestParameters
            {
                TotalRouteDistanceKm = 1000,
                FuelCapacity = 300,
                Consumption = 12.0,
                FinishFuel = 50
            };

            RunTest(stations, parameters);
        }

        /// <summary>
        /// Тест 3: Случай с ограниченным баком
        /// </summary>
        private static void TestLimitedTankCase()
        {
            Console.WriteLine("ТЕСТ 3: Ограниченный бак (малый объем)");
            Console.WriteLine("----------------------------------------");

            var stations = CreateLimitedTankTestStations();
            var parameters = new TestParameters
            {
                TotalRouteDistanceKm = 300,
                FuelCapacity = 80,
                Consumption = 10.0,
                FinishFuel = 20
            };

            RunTest(stations, parameters);
        }

        /// <summary>
        /// Запускает тест с заданными параметрами
        /// </summary>
        private static void RunTest(List<StationInfo> stations, TestParameters parameters)
        {
            Console.WriteLine($"Параметры маршрута:");
            Console.WriteLine($"  Общая длина: {parameters.TotalRouteDistanceKm} км");
            Console.WriteLine($"  Объем бака: {parameters.FuelCapacity} л");
            Console.WriteLine($"  Расход: {parameters.Consumption} л/100км");
            Console.WriteLine($"  Требуемый остаток: {parameters.FinishFuel} л");
            Console.WriteLine();

            Console.WriteLine("Доступные заправки:");
            for (int i = 0; i < stations.Count; i++)
            {
                var station = stations[i];
                Console.WriteLine($"  {i + 1}. {station.Station.ProviderName} - {station.ForwardDistanceKm} км, {station.PricePerLiter:F2} руб/л");
            }
            Console.WriteLine();

            // Вычисляем оптимальный маршрут
            var optimalStops = OptimalFuelStrategyFinder.CalculateOptimalStops(
                stations,
                parameters.TotalRouteDistanceKm,
                parameters.FuelCapacity,
                parameters.Consumption,
                parameters.FinishFuel);

            // Выводим результат
            if (optimalStops.Any())
            {
                Console.WriteLine("ОПТИМАЛЬНЫЙ МАРШРУТ:");
                Console.WriteLine("===================");

                double totalCost = 0;
                double totalRefilled = 0;

                for (int i = 0; i < optimalStops.Count; i++)
                {
                    var stop = optimalStops[i];
                    var station = stations.First(s => s.Station.Id == stop.Station.Id);
                    var cost = stop.RefillLiters * station.PricePerLiter;
                    totalCost += cost;
                    totalRefilled += stop.RefillLiters;

                    Console.WriteLine($"Остановка {i + 1}:");
                    Console.WriteLine($"  Станция: {stop.Station.ProviderName}");
                    Console.WriteLine($"  Расстояние: {stop.StopAtKm:F1} км");
                    Console.WriteLine($"  Заправлено: {stop.RefillLiters:F1} л");
                    Console.WriteLine($"  Остаток в баке: {stop.CurrentFuelLiters:F1} л");
                    Console.WriteLine($"  Стоимость: {cost:F2} руб.");
                    Console.WriteLine();
                }

                Console.WriteLine($"ИТОГО:");
                Console.WriteLine($"  Всего заправлено: {totalRefilled:F1} л");
                Console.WriteLine($"  Общая стоимость: {totalCost:F2} руб.");
                Console.WriteLine($"  Средняя цена: {(totalRefilled > 0 ? totalCost / totalRefilled : 0):F2} руб/л");
            }
            else
            {
                Console.WriteLine("Маршрут не найден!");
            }
        }

        /// <summary>
        /// Создает простые тестовые заправки
        /// </summary>
        private static List<StationInfo> CreateSimpleTestStations()
        {
            return new List<StationInfo>
            {
                new StationInfo { Station = CreateTestStation("Заправка А", 0), ForwardDistanceKm = 0, PricePerLiter = 2.5 },
                new StationInfo { Station = CreateTestStation("Заправка Б", 100), ForwardDistanceKm = 100, PricePerLiter = 2.3 },
                new StationInfo { Station = CreateTestStation("Заправка В", 200), ForwardDistanceKm = 200, PricePerLiter = 2.7 },
                new StationInfo { Station = CreateTestStation("Заправка Г", 300), ForwardDistanceKm = 300, PricePerLiter = 2.1 },
                new StationInfo { Station = CreateTestStation("Заправка Д", 400), ForwardDistanceKm = 400, PricePerLiter = 2.4 }
            };
        }

        /// <summary>
        /// Создает сложные тестовые заправки
        /// </summary>
        private static List<StationInfo> CreateComplexTestStations()
        {
            return new List<StationInfo>
            {
                new StationInfo { Station = CreateTestStation("Станция 1", 0), ForwardDistanceKm = 0, PricePerLiter = 2.8 },
                new StationInfo { Station = CreateTestStation("Станция 2", 50), ForwardDistanceKm = 50, PricePerLiter = 2.2 },
                new StationInfo { Station = CreateTestStation("Станция 3", 120), ForwardDistanceKm = 120, PricePerLiter = 2.9 },
                new StationInfo { Station = CreateTestStation("Станция 4", 180), ForwardDistanceKm = 180, PricePerLiter = 2.0 },
                new StationInfo { Station = CreateTestStation("Станция 5", 250), ForwardDistanceKm = 250, PricePerLiter = 2.6 },
                new StationInfo { Station = CreateTestStation("Станция 6", 320), ForwardDistanceKm = 320, PricePerLiter = 2.1 },
                new StationInfo { Station = CreateTestStation("Станция 7", 400), ForwardDistanceKm = 400, PricePerLiter = 2.7 },
                new StationInfo { Station = CreateTestStation("Станция 8", 480), ForwardDistanceKm = 480, PricePerLiter = 2.3 },
                new StationInfo { Station = CreateTestStation("Станция 9", 550), ForwardDistanceKm = 550, PricePerLiter = 2.4 },
                new StationInfo { Station = CreateTestStation("Станция 10", 620), ForwardDistanceKm = 620, PricePerLiter = 2.0 },
                new StationInfo { Station = CreateTestStation("Станция 11", 700), ForwardDistanceKm = 700, PricePerLiter = 2.8 },
                new StationInfo { Station = CreateTestStation("Станция 12", 780), ForwardDistanceKm = 780, PricePerLiter = 2.2 },
                new StationInfo { Station = CreateTestStation("Станция 13", 850), ForwardDistanceKm = 850, PricePerLiter = 2.5 },
                new StationInfo { Station = CreateTestStation("Станция 14", 920), ForwardDistanceKm = 920, PricePerLiter = 2.1 },
                new StationInfo { Station = CreateTestStation("Станция 15", 980), ForwardDistanceKm = 980, PricePerLiter = 2.6 }
            };
        }

        /// <summary>
        /// Создает тестовые заправки для случая с ограниченным баком
        /// </summary>
        private static List<StationInfo> CreateLimitedTankTestStations()
        {
            return new List<StationInfo>
            {
                new StationInfo { Station = CreateTestStation("Малая А", 0), ForwardDistanceKm = 0, PricePerLiter = 3.0 },
                new StationInfo { Station = CreateTestStation("Малая Б", 30), ForwardDistanceKm = 30, PricePerLiter = 2.8 },
                new StationInfo { Station = CreateTestStation("Малая В", 70), ForwardDistanceKm = 70, PricePerLiter = 2.5 },
                new StationInfo { Station = CreateTestStation("Малая Г", 120), ForwardDistanceKm = 120, PricePerLiter = 2.9 },
                new StationInfo { Station = CreateTestStation("Малая Д", 170), ForwardDistanceKm = 170, PricePerLiter = 2.2 },
                new StationInfo { Station = CreateTestStation("Малая Е", 220), ForwardDistanceKm = 220, PricePerLiter = 2.7 },
                new StationInfo { Station = CreateTestStation("Малая Ж", 270), ForwardDistanceKm = 270, PricePerLiter = 2.4 }
            };
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

        /// <summary>
        /// Параметры для тестирования
        /// </summary>
        private class TestParameters
        {
            public double TotalRouteDistanceKm { get; set; }
            public double FuelCapacity { get; set; }
            public double Consumption { get; set; }
            public double FinishFuel { get; set; }
        }
    }
}
