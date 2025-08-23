//using Foruscorp.FuelStations.Domain.FuelStations;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
//{
//    /// <summary>
//    /// Планировщик, который анализирует цепочки заправок для поиска оптимального маршрута
//    /// </summary>
//    public class ChainOptimizationPlanner
//    {
//        private const int MaxChainLength = 10000000; // Ограничиваем длину цепочки для производительности
//        private const double PriceThreshold = 0.1; // Минимальная разница в цене для рассмотрения альтернативы

//        /// <summary>
//        /// Находит оптимальную цепочку заправок и возвращает план остановок
//        /// </summary>
//        public StopPlanInfo FindOptimalChain(
//            List<StationInfo> stations,
//            FuelPlanningParameters parameters,
//            double currentFuel,
//            double currentPosition)
//        {
//            var reachableStations = stations;/*GetReachableStations(stations, currentPosition, currentFuel, parameters);*/


//            if (!reachableStations.Any())
//                return new StopPlanInfo { StopPlan = new List<FuelStopPlan>(), Finish = new FinishInfo() };

//            // Если заправок мало, используем простой алгоритм
//            if (reachableStations.Count <= 3)
//            {
//                var bestStation = reachableStations.OrderBy(s => s.PricePerLiter).First();
//                var stopPlan = CreateStopPlanFromStation(bestStation, parameters, currentFuel, currentPosition);
//                return new StopPlanInfo 
//                { 
//                    StopPlan = new List<FuelStopPlan> { stopPlan }, 
//                    Finish = CalculateFinishInfo(stopPlan, parameters) 
//                };
//            }

//            // Анализируем цепочки заправок
//            var chains = AnalyzeChains(reachableStations, parameters, currentFuel, currentPosition);
            
//            if (!chains.Any())
//            {
//                var bestStation = reachableStations.OrderBy(s => s.PricePerLiter).First();
//                var stopPlan = CreateStopPlanFromStation(bestStation, parameters, currentFuel, currentPosition);
//                return new StopPlanInfo 
//                { 
//                    StopPlan = new List<FuelStopPlan> { stopPlan }, 
//                    Finish = CalculateFinishInfo(stopPlan, parameters) 
//                };
//            }

//            // Выбираем самую выгодную цепочку, предпочитая полные цепочки
//            var bestChain = SelectBestChain(chains, parameters);
//            var stopPlans = CreateStopPlansFromChain(bestChain, parameters, currentFuel, currentPosition);
            
//            return new StopPlanInfo 
//            { 
//                StopPlan = stopPlans, 
//                Finish = CalculateFinishInfo(stopPlans, parameters) 
//            };
//        }

//        private List<StationInfo> GetReachableStations(
//            List<StationInfo> stations,
//            double currentPosition,
//            double currentFuel,
//            FuelPlanningParameters parameters)
//        {
//            var maxDistance = currentFuel / parameters.FuelConsumptionPerKm;
//            var maxReach = currentPosition + maxDistance;

//            return stations
//                .Where(s => s.Station != null &&
//                           s.ForwardDistanceKm > currentPosition &&
//                           s.ForwardDistanceKm <= maxReach)
//                .OrderBy(s => s.ForwardDistanceKm)
//                .ToList();
//        }

//        private List<FuelChain> AnalyzeChains(
//            List<StationInfo> reachableStations,
//            FuelPlanningParameters parameters,
//            double currentFuel,
//            double currentPosition)
//        {
//            var chains = new List<FuelChain>();
//            var visited = new HashSet<Guid>();

//            var taken = reachableStations;

//            // Начинаем с каждой доступной заправки
//            foreach (var startStation in taken) // Ограничиваем количество начальных точек
//            {
//                var chain = new FuelChain
//                {
//                    Stations = new List<StationInfo> { startStation },
//                    TotalCost = 0,
//                    RemainingFuel = currentFuel,
//                    CurrentPosition = currentPosition
//                };

//                ExploreChain(chain, reachableStations, parameters, chains, visited, 1);
//            }

//            return chains;
//        }

//        private void ExploreChain(
//            FuelChain currentChain,
//            List<StationInfo> allStations,
//            FuelPlanningParameters parameters,
//            List<FuelChain> allChains,
//            HashSet<Guid> visited,
//            int depth)
//        {
//            var lastStation = currentChain.Stations.Last();
            
//            // Проверяем, можем ли мы доехать до финиша с текущим топливом
//            var distanceToFinish = parameters.TotalDistanceKm - lastStation.ForwardDistanceKm;
//            var fuelNeededToFinish = distanceToFinish * parameters.FuelConsumptionPerKm + parameters.FinishFuel;
            
//            if (currentChain.RemainingFuel >= fuelNeededToFinish)
//            {
//                // Можем доехать до финиша - цепочка завершена
//                allChains.Add(currentChain.Clone());
//                return;
//            }

//            if (depth > MaxChainLength)
//            {
//                // Достигли максимальной глубины, но не можем доехать до финиша
//                // Добавляем цепочку как есть (неполную)
//                allChains.Add(currentChain.Clone());
//                return;
//            }

//            var maxRange = parameters.TankCapacity / parameters.FuelConsumptionPerKm;
//            var maxReach = lastStation.ForwardDistanceKm + maxRange;

//            // Находим следующие доступные заправки
//            var nextStations = allStations
//                .Where(s => s.Station != null &&
//                           s.ForwardDistanceKm > lastStation.ForwardDistanceKm &&
//                           s.ForwardDistanceKm <= maxReach &&
//                           !currentChain.Stations.Any(cs => cs.Station?.Id == s.Station?.Id))
//                .OrderBy(s => s.PricePerLiter)
//                .Take(3) // Ограничиваем количество вариантов для производительности
//                .ToList();

//            if (!nextStations.Any())
//            {
//                // Нет доступных заправок, но не можем доехать до финиша
//                // Добавляем цепочку как есть (неполную)
//                allChains.Add(currentChain.Clone());
//                return;
//            }

//            // Анализируем каждую следующую заправку
//            foreach (var nextStation in nextStations)
//            {
//                var newChain = currentChain.Clone();
                
//                // Рассчитываем стоимость перехода к следующей заправке
//                var distance = nextStation.ForwardDistanceKm - lastStation.ForwardDistanceKm;
//                var fuelNeeded = distance * parameters.FuelConsumptionPerKm;
                
//                // Рассчитываем, сколько нужно заправиться на текущей заправке
//                var refillAmount = CalculateOptimalRefillForChain(
//                    lastStation, nextStation, newChain.RemainingFuel, parameters);
                
//                var refillCost = refillAmount * lastStation.PricePerLiter;
//                newChain.TotalCost += refillCost;
                
//                // Обновляем состояние
//                newChain.RemainingFuel = newChain.RemainingFuel + refillAmount - fuelNeeded;
//                newChain.CurrentPosition = nextStation.ForwardDistanceKm;
//                newChain.Stations.Add(nextStation);

//                // Продолжаем исследование цепочки
//                //ExploreChain(newChain, allStations, parameters, allChains, visited, depth + 1);
//            }
//        }

//        private double CalculateOptimalRefillForChain(
//            StationInfo currentStation,
//            StationInfo nextStation,
//            double remainingFuel,
//            FuelPlanningParameters parameters)
//        {
//            var distance = nextStation.ForwardDistanceKm - currentStation.ForwardDistanceKm;
//            var fuelNeeded = distance * parameters.FuelConsumptionPerKm;
//            var minimumReserve = parameters.TankCapacity * 0.20;
            
//            var requiredFuel = fuelNeeded + minimumReserve;
//            var refillNeeded = Math.Max(0, requiredFuel - remainingFuel);
            
//            return Math.Min(refillNeeded, parameters.TankCapacity - remainingFuel);
//        }

//        private bool ShouldContinueChain(FuelChain chain, List<StationInfo> allStations, FuelPlanningParameters parameters)
//        {
//            var lastStation = chain.Stations.Last();
            
//            // Проверяем, есть ли более дешевые заправки впереди
//            var maxRange = parameters.TankCapacity / parameters.FuelConsumptionPerKm;
//            var maxReach = lastStation.ForwardDistanceKm + maxRange;
            
//            var cheaperAhead = allStations
//                .Where(s => s.Station != null &&
//                           s.ForwardDistanceKm > lastStation.ForwardDistanceKm &&
//                           s.ForwardDistanceKm <= maxReach &&
//                           s.PricePerLiter < lastStation.PricePerLiter - PriceThreshold)
//                .Any();

//                         return cheaperAhead;
//         }

//        /// <summary>
//        /// Выбирает лучшую цепочку, предпочитая полные цепочки
//        /// </summary>
//        private FuelChain SelectBestChain(List<FuelChain> chains, FuelPlanningParameters parameters)
//        {
//            // Разделяем цепочки на полные и неполные
//            var completeChains = new List<FuelChain>();
//            var incompleteChains = new List<FuelChain>();

//            foreach (var chain in chains)
//            {
//                if (chain.Stations.Any())
//                {
//                    var lastStation = chain.Stations.Last();
//                    var distanceToFinish = parameters.TotalDistanceKm - lastStation.ForwardDistanceKm;
//                    var fuelNeededToFinish = distanceToFinish * parameters.FuelConsumptionPerKm + parameters.FinishFuel;
                    
//                    if (chain.RemainingFuel >= fuelNeededToFinish)
//                    {
//                        completeChains.Add(chain);
//                    }
//                    else
//                    {
//                        incompleteChains.Add(chain);
//                    }
//                }
//                else
//                {
//                    incompleteChains.Add(chain);
//                }
//            }

//            // Предпочитаем полные цепочки
//            if (completeChains.Any())
//            {
//                return completeChains.OrderBy(c => c.TotalCost).First();
//            }

//            // Если полных цепочек нет, выбираем из неполных
//            if (incompleteChains.Any())
//            {
//                return incompleteChains.OrderBy(c => c.TotalCost).First();
//            }

//            // Если цепочек нет вообще, возвращаем пустую
//            return new FuelChain();
//        }

//        /// <summary>
//        /// Создает план остановки из одной заправки
//        /// </summary>
//        private FuelStopPlan CreateStopPlanFromStation(
//            StationInfo station,
//            FuelPlanningParameters parameters,
//            double currentFuel,
//            double currentPosition)
//        {
//            var distance = station.ForwardDistanceKm - currentPosition;
//            var fuelUsed = distance * parameters.FuelConsumptionPerKm;
//            var remainingFuel = currentFuel - fuelUsed;
            
//            // Рассчитываем оптимальное количество топлива для дозаправки
//            var refillAmount = CalculateOptimalRefillForSingleStation(station, remainingFuel, parameters);
            
//            return new FuelStopPlan
//            {
//                Station = station.Station!,
//                StopAtKm = station.ForwardDistanceKm,
//                RefillLiters = refillAmount,
//                CurrentFuelLiters = remainingFuel,
//                RoadSectionId = parameters.RoadSectionId
//            };
//        }

//        /// <summary>
//        /// Создает планы остановок из цепочки заправок
//        /// </summary>
//        private List<FuelStopPlan> CreateStopPlansFromChain(
//            FuelChain chain,
//            FuelPlanningParameters parameters,
//            double initialFuel,
//            double initialPosition)
//        {
//            var stopPlans = new List<FuelStopPlan>();
            
//            // Если цепочка пустая, возвращаем пустой список
//            if (!chain.Stations.Any())
//            {
//                return stopPlans;
//            }
            
//            var currentFuel = initialFuel;
//            var currentPosition = initialPosition;

//            for (int i = 0; i < chain.Stations.Count; i++)
//            {
//                var station = chain.Stations[i];
//                var distance = station.ForwardDistanceKm - currentPosition;
//                var fuelUsed = distance * parameters.FuelConsumptionPerKm;
                
//                // Проверяем, что у нас достаточно топлива
//                if (currentFuel < fuelUsed)
//                {
//                    currentFuel = 0;
//                }
//                else
//                {
//                    currentFuel -= fuelUsed;
//                }
                
//                // Рассчитываем количество топлива для дозаправки
//                double refillAmount = 0;
//                if (i < chain.Stations.Count - 1)
//                {
//                    // Для промежуточных остановок рассчитываем оптимальное количество
//                    var nextStation = chain.Stations[i + 1];
//                    refillAmount = CalculateOptimalRefillForChain(station, nextStation, currentFuel, parameters);
//                }
//                else
//                {
//                    // Для последней остановки проверяем, можем ли доехать до финиша
//                    var distanceToFinish = parameters.TotalDistanceKm - station.ForwardDistanceKm;
//                    var fuelNeededToFinish = distanceToFinish * parameters.FuelConsumptionPerKm + parameters.FinishFuel;
                    
//                    if (currentFuel + refillAmount >= fuelNeededToFinish)
//                    {
//                        // Можем доехать до финиша, заправляемся минимально необходимое
//                        refillAmount = Math.Max(0, fuelNeededToFinish - currentFuel);
//                    }
//                    else
//                    {
//                        // Не можем доехать до финиша, заправляемся до полного бака
//                        refillAmount = CalculateOptimalRefillForSingleStation(station, currentFuel, parameters);
//                    }
//                }

//                var stopPlan = new FuelStopPlan
//                {
//                    Station = station.Station!,
//                    StopAtKm = station.ForwardDistanceKm,
//                    RefillLiters = refillAmount,
//                    CurrentFuelLiters = currentFuel,
//                    RoadSectionId = parameters.RoadSectionId
//                };

//                stopPlans.Add(stopPlan);
                
//                // Обновляем состояние для следующей остановки
//                currentFuel += refillAmount;
//                currentPosition = station.ForwardDistanceKm;
//            }

//            return stopPlans;
//        }

//        /// <summary>
//        /// Рассчитывает оптимальное количество топлива для дозаправки на одной заправке
//        /// </summary>
//        private double CalculateOptimalRefillForSingleStation(
//            StationInfo station,
//            double remainingFuel,
//            FuelPlanningParameters parameters)
//        {
//            // Для упрощения, заправляемся до полного бака
//            // В реальной реализации здесь можно добавить логику поиска более дешевых заправок
//            var fullTankRefill = parameters.TankCapacity - remainingFuel;
//            return Math.Max(fullTankRefill, 0);
//        }

//        /// <summary>
//        /// Рассчитывает информацию о финальном состоянии
//        /// </summary>
//        private FinishInfo CalculateFinishInfo(FuelStopPlan stopPlan, FuelPlanningParameters parameters)
//        {
//            var fuelAfterRefill = stopPlan.CurrentFuelLiters + stopPlan.RefillLiters;
//            var distanceToFinish = parameters.TotalDistanceKm - stopPlan.StopAtKm;
//            var fuelUsedToFinish = distanceToFinish * parameters.FuelConsumptionPerKm;
//            var finalFuel = fuelAfterRefill - fuelUsedToFinish;
            
//            return new FinishInfo 
//            { 
//                RemainingFuelLiters = Math.Max(finalFuel, parameters.FinishFuel) 
//            };
//        }

//        /// <summary>
//        /// Рассчитывает информацию о финальном состоянии для списка остановок
//        /// </summary>
//        private FinishInfo CalculateFinishInfo(List<FuelStopPlan> stopPlans, FuelPlanningParameters parameters)
//        {
//            if (!stopPlans.Any())
//            {
//                // Если нет остановок, рассчитываем топливо от начального состояния
//                var fuelUsed = parameters.TotalDistanceKm * parameters.FuelConsumptionPerKm;
//                var finalFuelFinish = parameters.CurrentFuelLiters - fuelUsed;
//                return new FinishInfo { RemainingFuelLiters = Math.Max(finalFuelFinish, parameters.FinishFuel) };
//            }

//            // Симулируем прохождение всех остановок
//            var currentFuel = parameters.CurrentFuelLiters;
//            var currentPosition = 0.0;

//            foreach (var stop in stopPlans)
//            {
//                // Расходуем топливо до остановки
//                var distanceToStop = stop.StopAtKm - currentPosition;
//                var fuelUsed = distanceToStop * parameters.FuelConsumptionPerKm;
//                currentFuel -= fuelUsed;
                
//                // Заправляемся
//                currentFuel += stop.RefillLiters;
//                currentPosition = stop.StopAtKm;
//            }

//            // Расходуем топливо до финиша
//            var distanceToFinish = parameters.TotalDistanceKm - currentPosition;
//            var fuelUsedToFinish = distanceToFinish * parameters.FuelConsumptionPerKm;
//            var finalFuel = currentFuel - fuelUsedToFinish;

//            return new FinishInfo { RemainingFuelLiters = Math.Max(finalFuel, parameters.FinishFuel) };
//        }
//     }

//    /// <summary>
//    /// Представляет цепочку заправок с расчетом общей стоимости
//    /// </summary>
//    public class FuelChain
//    {
//        public List<StationInfo> Stations { get; set; } = new();
//        public double TotalCost { get; set; }
//        public double RemainingFuel { get; set; }
//        public double CurrentPosition { get; set; }

//        public FuelChain Clone()
//        {
//            return new FuelChain
//            {
//                Stations = new List<StationInfo>(Stations),
//                TotalCost = TotalCost,
//                RemainingFuel = RemainingFuel,
//                CurrentPosition = CurrentPosition
//            };
//        }
//    }

//    /// <summary>
//    /// Улучшенный планировщик, который использует анализ цепочек
//    /// </summary>
//    public class EnhancedFuelStopCalculator
//    {
//        private readonly ChainOptimizationPlanner _chainPlanner;

//        public EnhancedFuelStopCalculator()
//        {
//            _chainPlanner = new ChainOptimizationPlanner();
//        }

//        public StopPlanInfo FindNextOptimalStop(
//            List<StationInfo> stationInfos,
//            FuelPlanningParameters parameters,
//            HashSet<Guid> usedStationIds,
//            FuelState currentState,
//            double targetKm)
//        {
//            // Исключаем уже использованные заправки
//            var availableStations = stationInfos
//                .Where(si => si.Station != null && !usedStationIds.Contains(si.Station.Id))
//                .ToList();

//            // Находим оптимальную цепочку
//            var optimalChain = _chainPlanner.FindOptimalChain(
//                availableStations, parameters, currentState.RemainingFuel, currentState.PreviousKm);

//            return optimalChain;
//        }
//    }
//}
