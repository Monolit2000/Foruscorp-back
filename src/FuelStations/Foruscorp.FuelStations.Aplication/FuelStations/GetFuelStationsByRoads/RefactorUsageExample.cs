//using Foruscorp.FuelStations.Domain.FuelStations;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
//{
//    /// <summary>
//    /// Пример использования рефакторенного планировщика с демонстрацией рекурсивных алгоритмов
//    /// </summary>
//    public class RefactorUsageExample
//    {
//        public static void DemonstrateRefactoredPlanner()
//        {
//            Console.WriteLine("=== Демонстрация рефакторенного планировщика ===");
//            Console.WriteLine();

//            // Создаем рефакторенный планировщик
//            var planner = new RefactoredFuelStopStationPlanner();

//            // Подготавливаем тестовые данные
//            var route = CreateTestRoute();
//            var stations = CreateTestStations();

//            // Параметры планирования
//            var totalDistance = 1500.0; // км
//            var fuelConsumption = 0.08;  // л/км (8л/100км)
//            var currentFuel = 60.0;      // л
//            var tankCapacity = 200.0;    // л
//            var finishFuel = 40.0;       // л
//            var requiredStops = new List<RequiredStationDto>();

//            Console.WriteLine("📋 Параметры маршрута:");
//            Console.WriteLine($"   Общее расстояние: {totalDistance} км");
//            Console.WriteLine($"   Расход топлива: {fuelConsumption * 100} л/100км");
//            Console.WriteLine($"   Текущее топливо: {currentFuel} л");
//            Console.WriteLine($"   Емкость бака: {tankCapacity} л");
//            Console.WriteLine($"   Требуемое топливо на финише: {finishFuel} л");
//            Console.WriteLine($"   Количество заправок: {stations.Count}");
//            Console.WriteLine();

//            try
//            {
//                // Выполняем планирование
//                var startTime = DateTime.Now;
//                var result = planner.PlanStopsByStations(
//                    route, stations, totalDistance, fuelConsumption,
//                    currentFuel, tankCapacity, requiredStops, finishFuel);
//                var endTime = DateTime.Now;

//                // Выводим результаты
//                Console.WriteLine("✅ Планирование завершено успешно!");
//                Console.WriteLine($"⏱️  Время выполнения: {(endTime - startTime).TotalMilliseconds:F2} мс");
//                Console.WriteLine();

//                if (result.StopPlan.Any())
//                {
//                    Console.WriteLine("🛑 Запланированные остановки:");
//                    var totalCost = 0.0;

//                    for (int i = 0; i < result.StopPlan.Count; i++)
//                    {
//                        var stop = result.StopPlan[i];
//                        var cost = stop.RefillLiters * GetStationPrice(stop.Station);
//                        totalCost += cost;

//                        Console.WriteLine($"   {i + 1}. {stop.Station.ProviderName}");
//                        Console.WriteLine($"      📍 Позиция: {stop.StopAtKm:F1} км");
//                        Console.WriteLine($"      ⛽ Топливо при прибытии: {stop.CurrentFuelLiters:F1} л");
//                        Console.WriteLine($"      🔄 Дозаправка: {stop.RefillLiters:F1} л");
//                        Console.WriteLine($"      💰 Цена: ${GetStationPrice(stop.Station):F2}/л");
//                        Console.WriteLine($"      💸 Стоимость: ${cost:F2}");
//                        Console.WriteLine();
//                    }

//                    Console.WriteLine($"💵 Общая стоимость топлива: ${totalCost:F2}");
//                }
//                else
//                {
//                    Console.WriteLine("🚗 Остановки не требуются - можно доехать без дозаправки!");
//                }

//                Console.WriteLine($"🏁 Финальное топливо: {result.Finish.RemainingFuelLiters:F1} л");
//                Console.WriteLine();

//                // Демонстрируем рекурсивные компоненты
//                DemonstrateRecursiveComponents();
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Ошибка при планировании: {ex.Message}");
//            }
//        }

//        private static void DemonstrateRecursiveComponents()
//        {
//            Console.WriteLine("🔄 Демонстрация рекурсивных компонентов:");
//            Console.WriteLine();

//            // 1. Демонстрация рекурсивного анализа маршрута
//            Console.WriteLine("1️⃣  Рекурсивный анализ маршрута (RouteAnalyzer):");
//            var analyzer = new RouteAnalyzer();
//            var route = CreateTestRoute();
//            var stations = CreateTestStations();
            
//            var stationInfos = analyzer.AnalyzeStations(route, stations, 1500.0);
//            Console.WriteLine($"   ✅ Обработано {stations.Count} станций рекурсивно");
//            Console.WriteLine($"   📊 Найдено {stationInfos.Count} валидных станций");
//            Console.WriteLine();

//            // 2. Демонстрация рекурсивного выбора станций
//            Console.WriteLine("2️⃣  Рекурсивный выбор оптимальной станции:");
//            var selector = new OptimalStationSelector();
//            var context = CreateTestContext();
//            var state = new FuelState { CurrentPosition = 0.0, RemainingFuel = 60.0 };
            
//            var candidates = stationInfos.Take(5).ToList();
//            var selected = selector.SelectOptimalStation(candidates, state, context);
            
//            if (selected != null)
//            {
//                Console.WriteLine($"   🎯 Выбрана: {selected.Station?.ProviderName}");
//                Console.WriteLine($"   💰 Цена: ${selected.PricePerLiter:F2}/л");
//                Console.WriteLine($"   📍 Расстояние: {selected.ForwardDistanceKm:F1} км");
//            }
//            Console.WriteLine();

//            // 3. Демонстрация рекурсивного расчета дозаправки
//            Console.WriteLine("3️⃣  Рекурсивный расчет дозаправки:");
//            var refillCalculator = new SmartRefillCalculator();
            
//            if (selected != null)
//            {
//                var refillAmount = refillCalculator.CalculateOptimalRefill(
//                    selected, state, context, stationInfos);
                
//                Console.WriteLine($"   ⛽ Рекомендуемая дозаправка: {refillAmount:F1} л");
                
//                var nextCheaper = FindNextCheaperStation(stationInfos, selected);
//                if (nextCheaper != null)
//                {
//                    Console.WriteLine($"   🔍 Найдена более дешевая станция: {nextCheaper.Station?.ProviderName}");
//                    Console.WriteLine($"   💰 Цена: ${nextCheaper.PricePerLiter:F2}/л (экономия ${selected.PricePerLiter - nextCheaper.PricePerLiter:F2}/л)");
//                }
//            }
//            Console.WriteLine();

//            Console.WriteLine("🧠 Все алгоритмы используют рекурсию для:");
//            Console.WriteLine("   • Обхода структур данных без мутации состояния");
//            Console.WriteLine("   • Элегантной обработки базовых случаев");
//            Console.WriteLine("   • Композиции сложных алгоритмов из простых функций");
//            Console.WriteLine("   • Улучшения читаемости и тестируемости кода");
//        }

//        private static StationInfo? FindNextCheaperStation(List<StationInfo> stations, StationInfo current)
//        {
//            return stations
//                .Where(s => s.ForwardDistanceKm > current.ForwardDistanceKm && 
//                           s.PricePerLiter < current.PricePerLiter)
//                .OrderBy(s => s.PricePerLiter)
//                .FirstOrDefault();
//        }

//        private static List<GeoPoint> CreateTestRoute()
//        {
//            return new List<GeoPoint>
//            {
//                new GeoPoint(55.7558, 37.6176), // Москва
//                new GeoPoint(55.8000, 37.8000), // Промежуточная точка 1
//                new GeoPoint(56.0000, 38.0000), // Промежуточная точка 2
//                new GeoPoint(56.2000, 38.2000), // Промежуточная точка 3
//                new GeoPoint(59.9311, 30.3609)  // Санкт-Петербург
//            };
//        }

//        private static List<FuelStation> CreateTestStations()
//        {
//            return new List<FuelStation>
//            {
//                new FuelStation
//                {
//                    Id = Guid.NewGuid(),
//                    ProviderName = "Лукойл (близко, дорого)",
//                    Coordinates = new GeoPoint(55.7600, 37.6200),
//                    FuelPrices = new List<FuelPrice>
//                    {
//                        new FuelPrice { Price = 3.85, PriceAfterDiscount = 3.85 }
//                    }
//                },
//                new FuelStation
//                {
//                    Id = Guid.NewGuid(),
//                    ProviderName = "Shell (средне)",
//                    Coordinates = new GeoPoint(55.8100, 37.8100),
//                    FuelPrices = new List<FuelPrice>
//                    {
//                        new FuelPrice { Price = 3.45, PriceAfterDiscount = 3.45 }
//                    }
//                },
//                new FuelStation
//                {
//                    Id = Guid.NewGuid(),
//                    ProviderName = "Татнефть (дешево)",
//                    Coordinates = new GeoPoint(56.0100, 38.0100),
//                    FuelPrices = new List<FuelPrice>
//                    {
//                        new FuelPrice { Price = 3.15, PriceAfterDiscount = 3.15 }
//                    }
//                },
//                new FuelStation
//                {
//                    Id = Guid.NewGuid(),
//                    ProviderName = "Роснефть (очень дешево)",
//                    Coordinates = new GeoPoint(56.2100, 38.2100),
//                    FuelPrices = new List<FuelPrice>
//                    {
//                        new FuelPrice { Price = 2.95, PriceAfterDiscount = 2.95 }
//                    }
//                },
//                new FuelStation
//                {
//                    Id = Guid.NewGuid(),
//                    ProviderName = "Газпромнефть (финиш)",
//                    Coordinates = new GeoPoint(59.9400, 30.3700),
//                    FuelPrices = new List<FuelPrice>
//                    {
//                        new FuelPrice { Price = 3.25, PriceAfterDiscount = 3.25 }
//                    }
//                }
//            };
//        }

//        private static FuelPlanningContext CreateTestContext()
//        {
//            return new FuelPlanningContext
//            {
//                TotalDistanceKm = 1500.0,
//                FuelConsumptionPerKm = 0.08,
//                CurrentFuelLiters = 60.0,
//                TankCapacity = 200.0,
//                FinishFuel = 40.0,
//                RoadSectionId = "moscow-spb-route"
//            };
//        }

//        private static double GetStationPrice(FuelStation station)
//        {
//            return station.FuelPrices
//                .Where(fp => fp.PriceAfterDiscount > 0)
//                .OrderBy(fp => fp.PriceAfterDiscount)
//                .FirstOrDefault()?.PriceAfterDiscount ?? 0;
//        }
//    }

//    /// <summary>
//    /// Утилита для бенчмарка сравнения производительности
//    /// </summary>
//    public class PerformanceBenchmark
//    {
//        public static void ComparePerformance()
//        {
//            Console.WriteLine("⚡ Сравнение производительности:");
//            Console.WriteLine();

//            var route = RefactorUsageExample.CreateTestRoute();
//            var stations = RefactorUsageExample.CreateTestStations();

//            // Тест рефакторенного планировщика
//            var refactoredPlanner = new RefactoredFuelStopStationPlanner();
            
//            var times = new List<double>();
//            for (int i = 0; i < 10; i++)
//            {
//                var start = DateTime.Now;
//                var result = refactoredPlanner.PlanStopsByStations(
//                    route, stations, 1500.0, 0.08, 60.0, 200.0, 
//                    new List<RequiredStationDto>(), 40.0);
//                var end = DateTime.Now;
                
//                times.Add((end - start).TotalMilliseconds);
//            }

//            var avgTime = times.Average();
//            var minTime = times.Min();
//            var maxTime = times.Max();

//            Console.WriteLine($"🚀 Рефакторенный планировщик:");
//            Console.WriteLine($"   Среднее время: {avgTime:F2} мс");
//            Console.WriteLine($"   Минимальное время: {minTime:F2} мс");
//            Console.WriteLine($"   Максимальное время: {maxTime:F2} мс");
//            Console.WriteLine();

//            Console.WriteLine("📈 Улучшения:");
//            Console.WriteLine("   • Предсказуемая производительность");
//            Console.WriteLine("   • Контролируемое использование памяти");
//            Console.WriteLine("   • Линейная сложность большинства операций");
//            Console.WriteLine("   • Отсутствие утечек памяти через замыкания");
//        }
//    }
//}
