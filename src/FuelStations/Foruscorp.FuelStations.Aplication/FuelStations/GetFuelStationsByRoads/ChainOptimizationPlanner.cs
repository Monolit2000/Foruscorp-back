using Foruscorp.FuelStations.Domain.FuelStations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    /// <summary>
    /// Планировщик, который анализирует цепочки заправок для поиска оптимального маршрута
    /// </summary>
    public class ChainOptimizationPlanner
    {
        private const int MaxChainLength = 5; // Ограничиваем длину цепочки для производительности
        private const double PriceThreshold = 0.1; // Минимальная разница в цене для рассмотрения альтернативы

        /// <summary>
        /// Находит оптимальную цепочку заправок
        /// </summary>
        public List<StationInfo> FindOptimalChain(
            List<StationInfo> stations,
            FuelPlanningParameters parameters,
            double currentFuel,
            double currentPosition)
        {
            var reachableStations = GetReachableStations(stations, currentPosition, currentFuel, parameters);
            
            if (!reachableStations.Any())
                return new List<StationInfo>();

            // Если заправок мало, используем простой алгоритм
            if (reachableStations.Count <= 3)
            {
                return new List<StationInfo> { reachableStations.OrderBy(s => s.PricePerLiter).First() };
            }

            // Анализируем цепочки заправок
            var chains = AnalyzeChains(reachableStations, parameters, currentFuel, currentPosition);
            
            if (!chains.Any())
                return new List<StationInfo> { reachableStations.OrderBy(s => s.PricePerLiter).First() };

            // Выбираем самую выгодную цепочку
            var bestChain = chains.OrderBy(c => c.TotalCost).First();
            return bestChain.Stations;
        }

        private List<StationInfo> GetReachableStations(
            List<StationInfo> stations,
            double currentPosition,
            double currentFuel,
            FuelPlanningParameters parameters)
        {
            var maxDistance = currentFuel / parameters.FuelConsumptionPerKm;
            var maxReach = currentPosition + maxDistance;

            return stations
                .Where(s => s.Station != null &&
                           s.ForwardDistanceKm > currentPosition &&
                           s.ForwardDistanceKm <= maxReach)
                .OrderBy(s => s.ForwardDistanceKm)
                .ToList();
        }

        private List<FuelChain> AnalyzeChains(
            List<StationInfo> reachableStations,
            FuelPlanningParameters parameters,
            double currentFuel,
            double currentPosition)
        {
            var chains = new List<FuelChain>();
            var visited = new HashSet<Guid>();

            // Начинаем с каждой доступной заправки
            foreach (var startStation in reachableStations.Take(5)) // Ограничиваем количество начальных точек
            {
                var chain = new FuelChain
                {
                    Stations = new List<StationInfo> { startStation },
                    TotalCost = 0,
                    RemainingFuel = currentFuel,
                    CurrentPosition = currentPosition
                };

                ExploreChain(chain, reachableStations, parameters, chains, visited, 1);
            }

            return chains;
        }

        private void ExploreChain(
            FuelChain currentChain,
            List<StationInfo> allStations,
            FuelPlanningParameters parameters,
            List<FuelChain> allChains,
            HashSet<Guid> visited,
            int depth)
        {
            if (depth > MaxChainLength)
            {
                allChains.Add(currentChain.Clone());
                return;
            }

            var lastStation = currentChain.Stations.Last();
            var maxRange = parameters.TankCapacity / parameters.FuelConsumptionPerKm;
            var maxReach = lastStation.ForwardDistanceKm + maxRange;

            // Находим следующие доступные заправки
            var nextStations = allStations
                .Where(s => s.Station != null &&
                           s.ForwardDistanceKm > lastStation.ForwardDistanceKm &&
                           s.ForwardDistanceKm <= maxReach &&
                           !currentChain.Stations.Any(cs => cs.Station?.Id == s.Station?.Id))
                .OrderBy(s => s.PricePerLiter)
                .Take(3) // Ограничиваем количество вариантов для производительности
                .ToList();

            if (!nextStations.Any())
            {
                // Цепочка завершена
                allChains.Add(currentChain.Clone());
                return;
            }

            // Анализируем каждую следующую заправку
            foreach (var nextStation in nextStations)
            {
                var newChain = currentChain.Clone();
                
                // Рассчитываем стоимость перехода к следующей заправке
                var distance = nextStation.ForwardDistanceKm - lastStation.ForwardDistanceKm;
                var fuelNeeded = distance * parameters.FuelConsumptionPerKm;
                
                // Рассчитываем, сколько нужно заправиться на текущей заправке
                var refillAmount = CalculateOptimalRefillForChain(
                    lastStation, nextStation, newChain.RemainingFuel, parameters);
                
                var refillCost = refillAmount * lastStation.PricePerLiter;
                newChain.TotalCost += refillCost;
                
                // Обновляем состояние
                newChain.RemainingFuel = newChain.RemainingFuel + refillAmount - fuelNeeded;
                newChain.CurrentPosition = nextStation.ForwardDistanceKm;
                newChain.Stations.Add(nextStation);

                // Проверяем, стоит ли продолжать эту цепочку
                if (ShouldContinueChain(newChain, allStations, parameters))
                {
                    ExploreChain(newChain, allStations, parameters, allChains, visited, depth + 1);
                }
                else
                {
                    allChains.Add(newChain);
                }
            }
        }

        private double CalculateOptimalRefillForChain(
            StationInfo currentStation,
            StationInfo nextStation,
            double remainingFuel,
            FuelPlanningParameters parameters)
        {
            var distance = nextStation.ForwardDistanceKm - currentStation.ForwardDistanceKm;
            var fuelNeeded = distance * parameters.FuelConsumptionPerKm;
            var minimumReserve = parameters.TankCapacity * 0.20;
            
            var requiredFuel = fuelNeeded + minimumReserve;
            var refillNeeded = Math.Max(0, requiredFuel - remainingFuel);
            
            return Math.Min(refillNeeded, parameters.TankCapacity - remainingFuel);
        }

        private bool ShouldContinueChain(FuelChain chain, List<StationInfo> allStations, FuelPlanningParameters parameters)
        {
            var lastStation = chain.Stations.Last();
            
            // Проверяем, есть ли более дешевые заправки впереди
            var maxRange = parameters.TankCapacity / parameters.FuelConsumptionPerKm;
            var maxReach = lastStation.ForwardDistanceKm + maxRange;
            
            var cheaperAhead = allStations
                .Where(s => s.Station != null &&
                           s.ForwardDistanceKm > lastStation.ForwardDistanceKm &&
                           s.ForwardDistanceKm <= maxReach &&
                           s.PricePerLiter < lastStation.PricePerLiter - PriceThreshold)
                .Any();

            return cheaperAhead;
        }
    }

    /// <summary>
    /// Представляет цепочку заправок с расчетом общей стоимости
    /// </summary>
    public class FuelChain
    {
        public List<StationInfo> Stations { get; set; } = new();
        public double TotalCost { get; set; }
        public double RemainingFuel { get; set; }
        public double CurrentPosition { get; set; }

        public FuelChain Clone()
        {
            return new FuelChain
            {
                Stations = new List<StationInfo>(Stations),
                TotalCost = TotalCost,
                RemainingFuel = RemainingFuel,
                CurrentPosition = CurrentPosition
            };
        }
    }

    /// <summary>
    /// Улучшенный планировщик, который использует анализ цепочек
    /// </summary>
    public class EnhancedFuelStopCalculator
    {
        private readonly ChainOptimizationPlanner _chainPlanner;

        public EnhancedFuelStopCalculator()
        {
            _chainPlanner = new ChainOptimizationPlanner();
        }

        public StationInfo? FindNextOptimalStop(
            List<StationInfo> stationInfos,
            FuelPlanningParameters parameters,
            HashSet<Guid> usedStationIds,
            FuelState currentState,
            double targetKm)
        {
            // Исключаем уже использованные заправки
            var availableStations = stationInfos
                .Where(si => si.Station != null && !usedStationIds.Contains(si.Station.Id))
                .ToList();

            // Находим оптимальную цепочку
            var optimalChain = _chainPlanner.FindOptimalChain(
                availableStations, parameters, currentState.RemainingFuel, currentState.PreviousKm);

            return optimalChain.FirstOrDefault();
        }
    }
}
