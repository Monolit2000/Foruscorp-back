//using Foruscorp.FuelStations.Domain.FuelStations;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;

//namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
//{
//    /// <summary>
//    /// Бенчмарк для сравнения производительности алгоритмов оптимизации
//    /// Сравнивает ComprehensiveChainOptimizer vs DijkstraFuelOptimizer
//    /// </summary>
//    public class AlgorithmPerformanceBenchmark
//    {
//        private readonly ComprehensiveChainOptimizer _comprehensiveOptimizer;
//        private readonly DijkstraFuelOptimizer _dijkstraOptimizer;

//        public AlgorithmPerformanceBenchmark()
//        {
//            _comprehensiveOptimizer = new ComprehensiveChainOptimizer();
//            _dijkstraOptimizer = new DijkstraFuelOptimizer();
//        }

//        /// <summary>
//        /// Запускает полное сравнение производительности
//        /// </summary>
//        public BenchmarkResults RunFullBenchmark(
//            List<GeoPoint> route,
//            List<FuelStation> stations,
//            double totalDistanceKm,
//            double fuelConsumptionPerKm,
//            double currentFuelLiters,
//            double tankCapacity,
//            List<RequiredStationDto> requiredStops,
//            double finishFuel)
//        {
//            Console.WriteLine($"🚀 Запуск бенчмарка для {stations.Count} станций...");
//            Console.WriteLine($"📏 Маршрут: {totalDistanceKm:F0}км, Топливо: {currentFuelLiters:F0}л/{tankCapacity:F0}л");
//            Console.WriteLine();

//            var results = new BenchmarkResults
//            {
//                StationCount = stations.Count,
//                RouteDistance = totalDistanceKm
//            };

//            // 1. Тестируем Comprehensive алгоритм
//            Console.WriteLine("📊 Тестирование ComprehensiveChainOptimizer...");
//            results.ComprehensiveResult = BenchmarkAlgorithm(
//                () => _comprehensiveOptimizer.FindOptimalChainComprehensive(
//                    route, stations, totalDistanceKm, fuelConsumptionPerKm,
//                    currentFuelLiters, tankCapacity, requiredStops, finishFuel),
//                "Comprehensive");

//            // 2. Тестируем Dijkstra алгоритм  
//            Console.WriteLine("🗺️ Тестирование DijkstraFuelOptimizer...");
//            results.DijkstraResult = BenchmarkAlgorithm(
//                () => _dijkstraOptimizer.FindOptimalRoute(
//                    route, stations, totalDistanceKm, fuelConsumptionPerKm,
//                    currentFuelLiters, tankCapacity, requiredStops, finishFuel),
//                "Dijkstra");

//            // 3. Анализируем результаты
//            AnalyzeResults(results);

//            return results;
//        }

//        /// <summary>
//        /// Бенчмарк отдельного алгоритма
//        /// </summary>
//        private AlgorithmResult BenchmarkAlgorithm(Func<StopPlanInfo> algorithm, string algorithmName)
//        {
//            var stopwatch = Stopwatch.StartNew();
//            var memoryBefore = GC.GetTotalMemory(false);

//            StopPlanInfo result = null;
//            Exception error = null;

//            try
//            {
//                result = algorithm();
//            }
//            catch (Exception ex)
//            {
//                error = ex;
//                Console.WriteLine($"❌ Ошибка в {algorithmName}: {ex.Message}");
//            }

//            stopwatch.Stop();
//            var memoryAfter = GC.GetTotalMemory(false);

//            var algorithmResult = new AlgorithmResult
//            {
//                AlgorithmName = algorithmName,
//                ExecutionTimeMs = stopwatch.ElapsedMilliseconds,
//                MemoryUsedBytes = memoryAfter - memoryBefore,
//                Success = result != null && error == null,
//                Error = error?.Message,
//                StopPlan = result
//            };

//            if (result != null)
//            {
//                algorithmResult.StopsCount = result.StopPlan.Count;
//                algorithmResult.TotalFuelCost = CalculateTotalCost(result);
//            }

//            Console.WriteLine($"⏱️ {algorithmName}: {algorithmResult.ExecutionTimeMs}ms, " +
//                             $"Память: {algorithmResult.MemoryUsedBytes / 1024:F0}KB, " +
//                             $"Остановок: {algorithmResult.StopsCount}");

//            return algorithmResult;
//        }

//        /// <summary>
//        /// Анализирует и сравнивает результаты
//        /// </summary>
//        private void AnalyzeResults(BenchmarkResults results)
//        {
//            Console.WriteLine();
//            Console.WriteLine("📈 СРАВНИТЕЛЬНЫЙ АНАЛИЗ");
//            Console.WriteLine("=" * 50);

//            var comp = results.ComprehensiveResult;
//            var dijkstra = results.DijkstraResult;

//            // Производительность
//            Console.WriteLine($"⏱️ ВРЕМЯ ВЫПОЛНЕНИЯ:");
//            Console.WriteLine($"   ComprehensiveChainOptimizer: {comp.ExecutionTimeMs}ms");
//            Console.WriteLine($"   DijkstraFuelOptimizer:       {dijkstra.ExecutionTimeMs}ms");
            
//            if (comp.Success && dijkstra.Success)
//            {
//                var speedup = (double)comp.ExecutionTimeMs / dijkstra.ExecutionTimeMs;
//                Console.WriteLine($"   🚀 Ускорение Dijkstra: {speedup:F1}x");
//            }

//            // Память
//            Console.WriteLine($"\n💾 ИСПОЛЬЗОВАНИЕ ПАМЯТИ:");
//            Console.WriteLine($"   ComprehensiveChainOptimizer: {comp.MemoryUsedBytes / 1024:F0}KB");
//            Console.WriteLine($"   DijkstraFuelOptimizer:       {dijkstra.MemoryUsedBytes / 1024:F0}KB");

//            // Качество решения
//            Console.WriteLine($"\n💰 КАЧЕСТВО РЕШЕНИЯ:");
//            if (comp.Success && dijkstra.Success)
//            {
//                Console.WriteLine($"   ComprehensiveChainOptimizer: {comp.StopsCount} остановок, ${comp.TotalFuelCost:F2}");
//                Console.WriteLine($"   DijkstraFuelOptimizer:       {dijkstra.StopsCount} остановок, ${dijkstra.TotalFuelCost:F2}");

//                var costDifference = dijkstra.TotalFuelCost - comp.TotalFuelCost;
//                var costDifferencePercent = (costDifference / comp.TotalFuelCost) * 100;
                
//                Console.WriteLine($"   💡 Разница в стоимости: {costDifference:+F2;-F2}$ ({costDifferencePercent:+F1;-F1}%)");
//            }

//            // Сложность
//            Console.WriteLine($"\n🧮 ТЕОРЕТИЧЕСКАЯ СЛОЖНОСТЬ:");
//            Console.WriteLine($"   ComprehensiveChainOptimizer: O(min(1M, 16^k × k!)) где k≤10");
//            Console.WriteLine($"   DijkstraFuelOptimizer:       O((V + E) log V) где V≈4n, E≈n²");

//            // Рекомендации
//            Console.WriteLine($"\n🎯 РЕКОМЕНДАЦИИ:");
//            if (results.StationCount <= 20)
//            {
//                Console.WriteLine($"   ✅ Для {results.StationCount} станций оба алгоритма эффективны");
//                Console.WriteLine($"   💡 Dijkstra быстрее, Comprehensive точнее");
//            }
//            else if (results.StationCount <= 50)
//            {
//                Console.WriteLine($"   ⚠️ Для {results.StationCount} станций рекомендуется Dijkstra");
//                Console.WriteLine($"   🚀 Значительно быстрее с приемлемым качеством");
//            }
//            else
//            {
//                Console.WriteLine($"   🚨 Для {results.StationCount} станций только Dijkstra");
//                Console.WriteLine($"   ❌ Comprehensive будет слишком медленным");
//            }
//        }

//        /// <summary>
//        /// Рассчитывает общую стоимость топлива
//        /// </summary>
//        private double CalculateTotalCost(StopPlanInfo stopPlan)
//        {
//            return stopPlan.StopPlan.Sum(stop => 
//            {
//                var price = stop.Station.FuelPrices
//                    .Where(fp => fp.PriceAfterDiscount > 0)
//                    .OrderBy(fp => fp.PriceAfterDiscount)
//                    .FirstOrDefault()?.PriceAfterDiscount ?? 0;
                    
//                return stop.RefillLiters * price;
//            });
//        }

//        /// <summary>
//        /// Генерирует стресс-тест с заданным количеством станций
//        /// </summary>
//        public static BenchmarkResults RunStressTest(int stationCount)
//        {
//            var testData = GenerateTestData(stationCount);
//            var benchmark = new AlgorithmPerformanceBenchmark();
            
//            return benchmark.RunFullBenchmark(
//                testData.Route,
//                testData.Stations,
//                testData.TotalDistance,
//                testData.FuelConsumption,
//                testData.CurrentFuel,
//                testData.TankCapacity,
//                new List<RequiredStationDto>(),
//                testData.FinishFuel);
//        }

//        /// <summary>
//        /// Генерирует тестовые данные
//        /// </summary>
//        private static TestData GenerateTestData(int stationCount)
//        {
//            var random = new Random(42); // Фиксированный seed для воспроизводимости
//            var totalDistance = 1000.0; // 1000км маршрут
            
//            // Генерируем маршрут
//            var route = new List<GeoPoint>();
//            for (int i = 0; i <= 10; i++)
//            {
//                route.Add(new GeoPoint 
//                { 
//                    Latitude = 55.7558 + i * 0.1, 
//                    Longitude = 37.6176 + i * 0.1 
//                });
//            }

//            // Генерируем станции
//            var stations = new List<FuelStation>();
//            for (int i = 0; i < stationCount; i++)
//            {
//                var distance = random.NextDouble() * totalDistance;
//                var price = 1.40 + random.NextDouble() * 0.60; // $1.40-$2.00

//                stations.Add(new FuelStation
//                {
//                    Id = i + 1,
//                    ProviderName = $"Station_{i + 1}",
//                    Coordinates = new GeoPoint 
//                    { 
//                        Latitude = 55.7558 + (distance / 100), 
//                        Longitude = 37.6176 + (distance / 100) 
//                    },
//                    FuelPrices = new List<FuelPrice>
//                    {
//                        new FuelPrice { PriceAfterDiscount = price }
//                    }
//                });
//            }

//            return new TestData
//            {
//                Route = route,
//                Stations = stations,
//                TotalDistance = totalDistance,
//                FuelConsumption = 0.35, // 35л/100км
//                CurrentFuel = 60, // 60л в баке
//                TankCapacity = 200, // 200л бак
//                FinishFuel = 40 // 40л на финише
//            };
//        }
//    }

//    #region Data Structures

//    /// <summary>
//    /// Результаты бенчмарка
//    /// </summary>
//    public class BenchmarkResults
//    {
//        public int StationCount { get; set; }
//        public double RouteDistance { get; set; }
//        public AlgorithmResult ComprehensiveResult { get; set; }
//        public AlgorithmResult DijkstraResult { get; set; }
//    }

//    /// <summary>
//    /// Результат отдельного алгоритма
//    /// </summary>
//    public class AlgorithmResult
//    {
//        public string AlgorithmName { get; set; }
//        public long ExecutionTimeMs { get; set; }
//        public long MemoryUsedBytes { get; set; }
//        public bool Success { get; set; }
//        public string Error { get; set; }
//        public int StopsCount { get; set; }
//        public double TotalFuelCost { get; set; }
//        public StopPlanInfo StopPlan { get; set; }
//    }

//    /// <summary>
//    /// Тестовые данные
//    /// </summary>
//    public class TestData
//    {
//        public List<GeoPoint> Route { get; set; }
//        public List<FuelStation> Stations { get; set; }
//        public double TotalDistance { get; set; }
//        public double FuelConsumption { get; set; }
//        public double CurrentFuel { get; set; }
//        public double TankCapacity { get; set; }
//        public double FinishFuel { get; set; }
//    }

//    #endregion
//}
