using System;
using System.Collections.Generic;
using System.Linq;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    /// <summary>
    /// Умный калькулятор стоимости цепочки заправок
    /// Учитывает цену топлива, количество дозаправки, расстояния и эффективность
    /// </summary>
    public class SmartChainCostCalculator : IChainCostCalculator
    {
        /// <summary>
        /// Рассчитывает полную стоимость цепочки заправок
        /// </summary>
        public ChainCost CalculateChainCost(FuelChain chain, FuelPlanningContext context)
        {
            var chainCost = new ChainCost();
            var currentFuel = context.CurrentFuelLiters;
            var currentPosition = 0.0;

            // Если цепочка пустая (без остановок)
            if (!chain.Stations.Any())
            {
                return CalculateNoStopsChainCost(context);
            }

            // Рассчитываем стоимость для каждой остановки
            foreach (var station in chain.Stations)
            {
                var stopCost = CalculateStopCost(
                    station, currentFuel, currentPosition, 
                    chain.Stations, context);

                chainCost.StopCosts.Add(stopCost);
                chainCost.TotalFuelCost += stopCost.RefillCost;
                chainCost.TotalRefillAmount += stopCost.RefillAmount;
                chainCost.TotalDistance += stopCost.DistanceFromPrevious;

                // Обновляем состояние для следующей итерации
                currentFuel = currentFuel - (stopCost.DistanceFromPrevious * context.FuelConsumptionPerKm) + stopCost.RefillAmount;
                currentPosition = station.ForwardDistanceKm;
            }

            // Рассчитываем итоговые метрики
            chainCost.EfficiencyScore = CalculateEfficiencyScore(chainCost, context);
            chainCost.TotalScore = CalculateTotalScore(chainCost, context);

            return chainCost;
        }

        /// <summary>
        /// Рассчитывает стоимость цепочки без остановок
        /// </summary>
        private ChainCost CalculateNoStopsChainCost(FuelPlanningContext context)
        {
            var totalFuelNeeded = context.TotalDistanceKm * context.FuelConsumptionPerKm + context.FinishFuel;
            
            if (context.CurrentFuelLiters >= totalFuelNeeded)
            {
                return new ChainCost
                {
                    TotalFuelCost = 0, // Не тратим деньги на заправку
                    TotalRefillAmount = 0,
                    TotalDistance = context.TotalDistanceKm,
                    EfficiencyScore = 100, // Максимальная эффективность
                    TotalScore = 0 // Лучший возможный балл
                };
            }
            else
            {
                // Невозможно доехать без заправки
                return new ChainCost
                {
                    TotalFuelCost = double.MaxValue,
                    EfficiencyScore = 0,
                    TotalScore = double.MaxValue
                };
            }
        }

        /// <summary>
        /// Рассчитывает стоимость отдельной остановки
        /// </summary>
        private StopCost CalculateStopCost(
            StationInfo station,
            double currentFuel,
            double currentPosition,
            List<StationInfo> allStationsInChain,
            FuelPlanningContext context)
        {
            var distance = station.ForwardDistanceKm - currentPosition;
            var fuelUsed = distance * context.FuelConsumptionPerKm;
            var fuelAtArrival = currentFuel - fuelUsed;

            // Рассчитываем оптимальную дозаправку
            var refillAmount = CalculateOptimalRefillAmount(
                station, fuelAtArrival, allStationsInChain, context);

            var refillCost = refillAmount * station.PricePerLiter;
            var fuelEfficiency = CalculateFuelEfficiency(
                station, refillAmount, distance, context);

            return new StopCost
            {
                Station = station,
                RefillAmount = refillAmount,
                RefillCost = refillCost,
                DistanceFromPrevious = distance,
                FuelEfficiency = fuelEfficiency
            };
        }

        /// <summary>
        /// Рассчитывает оптимальное количество дозаправки на станции
        /// </summary>
        public double CalculateOptimalRefillAmount(
            StationInfo station,
            double fuelAtArrival,
            List<StationInfo> remainingStations,
            FuelPlanningContext context)
        {
            // Находим следующую станцию в цепочке после текущей
            var nextStation = remainingStations
                .Where(s => s.ForwardDistanceKm > station.ForwardDistanceKm)
                .OrderBy(s => s.ForwardDistanceKm)
                .FirstOrDefault();

            if (nextStation == null)
            {
                // Это последняя станция - заправляемся для финиша
                return CalculateRefillForFinish(station, fuelAtArrival, context);
            }
            else
            {
                // Есть следующая станция - оптимизируем дозаправку
                return CalculateRefillForNextStation(
                    station, nextStation, fuelAtArrival, context);
            }
        }

        /// <summary>
        /// Рассчитывает дозаправку для достижения финиша
        /// </summary>
        private double CalculateRefillForFinish(
            StationInfo station,
            double fuelAtArrival,
            FuelPlanningContext context)
        {
            var distanceToFinish = context.TotalDistanceKm - station.ForwardDistanceKm;
            var fuelNeededToFinish = distanceToFinish * context.FuelConsumptionPerKm + context.FinishFuel;
            var requiredRefill = fuelNeededToFinish - fuelAtArrival;

            // Ограничиваем максимальной вместимостью бака
            var maxRefill = context.TankCapacity - fuelAtArrival;
            
            return Math.Max(0, Math.Min(requiredRefill, maxRefill));
        }

        /// <summary>
        /// Рассчитывает дозаправку для достижения следующей станции
        /// </summary>
        private double CalculateRefillForNextStation(
            StationInfo currentStation,
            StationInfo nextStation,
            double fuelAtArrival,
            FuelPlanningContext context)
        {
            var distanceToNext = nextStation.ForwardDistanceKm - currentStation.ForwardDistanceKm;
            var fuelNeededToNext = distanceToNext * context.FuelConsumptionPerKm;
            var minReserve = context.TankCapacity * FuelPlanningConfig.MinReserveFactor;

            // Стратегия дозаправки зависит от сравнения цен
            var currentPrice = currentStation.PricePerLiter;
            var nextPrice = nextStation.PricePerLiter;

            if (currentPrice <= nextPrice * 1.05) // Текущая цена не более чем на 5% выше
            {
                // Заправляемся максимально, т.к. цена выгодная
                var maxRefill = context.TankCapacity - fuelAtArrival;
                return Math.Max(0, maxRefill);
            }
            else
            {
                // Заправляемся минимально, чтобы дойти до следующей станции
                var minRefill = fuelNeededToNext + minReserve - fuelAtArrival;
                var maxRefill = context.TankCapacity - fuelAtArrival;
                
                return Math.Max(0, Math.Min(minRefill, maxRefill));
            }
        }

        /// <summary>
        /// Рассчитывает эффективность использования топлива
        /// </summary>
        private double CalculateFuelEfficiency(
            StationInfo station,
            double refillAmount,
            double distance,
            FuelPlanningContext context)
        {
            if (refillAmount <= 0 || distance <= 0)
                return 0;

            // Эффективность = километры на литр дозаправки
            var kmPerLiter = distance / refillAmount;
            
            // Нормализуем относительно теоретического оптимума
            var theoreticalOptimal = 1.0 / context.FuelConsumptionPerKm;
            
            return Math.Min(100, (kmPerLiter / theoreticalOptimal) * 100);
        }

        /// <summary>
        /// Рассчитывает общую эффективность цепочки
        /// </summary>
        private double CalculateEfficiencyScore(ChainCost chainCost, FuelPlanningContext context)
        {
            if (!chainCost.StopCosts.Any())
                return 100; // Максимальная эффективность для цепочки без остановок

            // Средняя эффективность всех остановок
            var avgEfficiency = chainCost.StopCosts.Average(sc => sc.FuelEfficiency);

            // Бонус за меньшее количество остановок
            var stopPenalty = chainCost.StopCosts.Count * 5; // 5 баллов штрафа за каждую остановку
            
            // Бонус за использование дешевых станций
            var avgPrice = chainCost.StopCosts.Average(sc => sc.Station.PricePerLiter);
            var cheapestPrice = chainCost.StopCosts.Min(sc => sc.Station.PricePerLiter);
            var priceEfficiency = (cheapestPrice / avgPrice) * 100;

            return Math.Max(0, (avgEfficiency + priceEfficiency) / 2 - stopPenalty);
        }

        /// <summary>
        /// Рассчитывает итоговый балл для сравнения цепочек
        /// </summary>
        private double CalculateTotalScore(ChainCost chainCost, FuelPlanningContext context)
        {
            // Основные компоненты балла:
            // 1. Стоимость топлива (чем меньше, тем лучше)
            // 2. Количество остановок (чем меньше, тем лучше)
            // 3. Эффективность (чем больше, тем лучше)

            var fuelCostComponent = chainCost.TotalFuelCost;
            var stopsComponent = chainCost.StopCosts.Count * 50; // Штраф за каждую остановку
            var efficiencyComponent = -(chainCost.EfficiencyScore * 2); // Бонус за эффективность

            // Дополнительные факторы
            var distanceComponent = Math.Abs(chainCost.TotalDistance - context.TotalDistanceKm) * 0.1;
            var refillComponent = chainCost.TotalRefillAmount * 0.05; // Небольшой штраф за большие дозаправки

            return fuelCostComponent + stopsComponent + efficiencyComponent + distanceComponent + refillComponent;
        }
    }
}
