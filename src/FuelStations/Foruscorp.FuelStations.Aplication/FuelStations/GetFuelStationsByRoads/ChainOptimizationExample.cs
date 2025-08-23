//using Foruscorp.FuelStations.Domain.FuelStations;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
//{
//    /// <summary>
//    /// Пример использования алгоритма оптимизации цепочек заправок
//    /// </summary>
//    public class ChainOptimizationExample
//    {
//        public static void DemonstrateChainOptimization()
//        {
//            // Создаем тестовые данные (ваш пример)
//            var stations = CreateTestStations();
//            var parameters = CreateTestParameters();
            
//            var planner = new ChainOptimizationPlanner();
            
//            // Анализируем оптимальную цепочку
//            var optimalChain = planner.FindOptimalChain(
//                stations, 
//                parameters, 
//                currentFuel: 50.0, // 50 литров в баке
//                currentPosition: 0.0); // Начальная позиция
            
//            Console.WriteLine("=== Анализ оптимальной цепочки заправок ===");
//            Console.WriteLine($"Найдено остановок в плане: {optimalChain.StopPlan.Count}");
//            Console.WriteLine($"Финальное топливо: {optimalChain.Finish.RemainingFuelLiters:F2} л");
            
//            if (optimalChain.StopPlan.Any())
//            {
//                Console.WriteLine("\nОптимальный план остановок:");
//                for (int i = 0; i < optimalChain.StopPlan.Count; i++)
//                {
//                    var stop = optimalChain.StopPlan[i];
//                    Console.WriteLine($"{i + 1}. {stop.Station.ProviderName} - {stop.RefillLiters:F2}л на км {stop.StopAtKm:F1} (цена: {GetStationPrice(stop.Station):C}/л)");
//                }
                
//                // Рассчитываем общую стоимость
//                var totalCost = optimalChain.StopPlan.Sum(stop => 
//                    stop.RefillLiters * GetStationPrice(stop.Station));
//                Console.WriteLine($"\nОбщая стоимость топлива: {totalCost:C}");
//            }
            
//            // Сравниваем с простым алгоритмом
//            var simpleChoice = stations.OrderBy(s => s.PricePerLiter).First();
//            Console.WriteLine($"\nПростой алгоритм выбрал бы: {simpleChoice.Station?.ProviderName} - {simpleChoice.PricePerLiter:C}/л");
            
//            if (optimalChain.StopPlan.Any())
//            {
//                var chainChoice = optimalChain.StopPlan.First();
//                var chainPrice = GetStationPrice(chainChoice.Station);
//                Console.WriteLine($"Алгоритм цепочек выбрал: {chainChoice.Station.ProviderName} - {chainPrice:C}/л");
                
//                if (chainPrice > simpleChoice.PricePerLiter)
//                {
//                    Console.WriteLine("Алгоритм цепочек выбрал более дорогую заправку, но это может быть выгоднее в долгосрочной перспективе!");
//                }
//            }
//        }
        
//        private static double GetStationPrice(FuelStation station)
//        {
//            return station.FuelPrices
//                .Where(fp => fp.PriceAfterDiscount >= 0)
//                .OrderBy(fp => fp.PriceAfterDiscount)
//                .FirstOrDefault()?.PriceAfterDiscount ?? double.MaxValue;
//        }
        
//        private static List<StationInfo> CreateTestStations()
//        {
//            return new List<StationInfo>
//            {
//                new StationInfo
//                {
//                    Station = new FuelStation 
//                    { 
//                        Id = Guid.NewGuid(), 
//                        ProviderName = "Заправка A (дорогая)",
//                        Coordinates = new GeoPoint(0, 0),
//                        FuelPrices = new List<FuelPrice> 
//                        { 
//                            new FuelPrice { Price = 4.30, PriceAfterDiscount = 4.30 } 
//                        }
//                    },
//                    ForwardDistanceKm = 50.0,
//                    PricePerLiter = 4.30
//                },
//                new StationInfo
//                {
//                    Station = new FuelStation 
//                    { 
//                        Id = Guid.NewGuid(), 
//                        ProviderName = "Заправка B (средняя)",
//                        Coordinates = new GeoPoint(0, 0),
//                        FuelPrices = new List<FuelPrice> 
//                        { 
//                            new FuelPrice { Price = 3.10, PriceAfterDiscount = 3.10 } 
//                        }
//                    },
//                    ForwardDistanceKm = 150.0,
//                    PricePerLiter = 3.10
//                },
//                new StationInfo
//                {
//                    Station = new FuelStation 
//                    { 
//                        Id = Guid.NewGuid(), 
//                        ProviderName = "Заправка C (дешевая, но далеко)",
//                        Coordinates = new GeoPoint(0, 0),
//                        FuelPrices = new List<FuelPrice> 
//                        { 
//                            new FuelPrice { Price = 2.90, PriceAfterDiscount = 2.90 } 
//                        }
//                    },
//                    ForwardDistanceKm = 300.0,
//                    PricePerLiter = 2.90
//                },
//                new StationInfo
//                {
//                    Station = new FuelStation 
//                    { 
//                        Id = Guid.NewGuid(), 
//                        ProviderName = "Заправка D (еще дешевле)",
//                        Coordinates = new GeoPoint(0, 0),
//                        FuelPrices = new List<FuelPrice> 
//                        { 
//                            new FuelPrice { Price = 2.70, PriceAfterDiscount = 2.70 } 
//                        }
//                    },
//                    ForwardDistanceKm = 400.0,
//                    PricePerLiter = 2.70
//                }
//            };
//        }
        
//        private static FuelPlanningParameters CreateTestParameters()
//        {
//            return new FuelPlanningParameters
//            {
//                CurrentFuelLiters = 50.0,
//                TankCapacity = 200.0,
//                FuelConsumptionPerKm = 0.1, // 10 литров на 100 км
//                FinishFuel = 40.0,
//                TotalDistanceKm = 500.0,
//                RequiredStops = new List<RequiredStationDto>(),
//                RoadSectionId = "test"
//            };
//        }
//    }
//}
