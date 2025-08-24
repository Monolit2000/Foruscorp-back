using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    /// <summary>
    /// Комплексный валидатор цепочек заправок
    /// Проверяет достижимость финиша, соблюдение 20% запаса, минимальные расстояния
    /// </summary>
    public class ComprehensiveChainValidator : IChainValidator
    {
        /// <summary>
        /// Проверяет, является ли цепочка валидной
        /// </summary>
        public bool IsChainValid(FuelChain chain, FuelPlanningContext context)
        {
            try
            {
                var result = ValidateChainDetailed(chain, context);
                return result.IsValid;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Возвращает детальную информацию о валидности цепочки
        /// </summary>
        public ChainValidationResult ValidateChainDetailed(FuelChain chain, FuelPlanningContext context)
        {
            var result = new ChainValidationResult { IsValid = true };
            
            try
            {
                // Проверка 1: Валидация цепочки без остановок
                if (!chain.Stations.Any())
                {
                    return ValidateNoStopsChain(context);
                }

                // Проверка 2: Упорядоченность станций по расстоянию
                if (!ValidateStationOrder(chain, result))
                    return result;

                // Проверка 3: Минимальные расстояния между станциями
                if (!ValidateMinimumDistances(chain, context, result))
                    return result;

                // Проверка 4: Симуляция поездки с проверкой всех ограничений
                if (!SimulateTripAndValidate(chain, context, result))
                    return result;

                // Если все проверки пройдены
                result.IsValid = true;
                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.FailureReason = $"Ошибка валидации: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Валидация цепочки без остановок
        /// </summary>
        private ChainValidationResult ValidateNoStopsChain(FuelPlanningContext context)
        {
            var result = new ChainValidationResult();
            
            var totalFuelNeeded = context.TotalDistanceKm * context.FuelConsumptionPerKm + context.FinishFuel;
            var minReserve = context.TankCapacity * FuelPlanningConfig.MinReserveFactor;
            
            // Проверяем, хватит ли топлива с учетом 20% запаса
            if (context.CurrentFuelLiters >= totalFuelNeeded + minReserve)
            {
                result.IsValid = true;
                result.FinalFuelAmount = context.CurrentFuelLiters - totalFuelNeeded;
                
                result.StepResults.Add(new ValidationStepResult
                {
                    Station = null,
                    FuelBefore = context.CurrentFuelLiters,
                    FuelAfter = result.FinalFuelAmount,
                    RefillAmount = 0,
                    Distance = context.TotalDistanceKm,
                    MeetsReserveRequirement = true,
                    Notes = "Прямая поездка до финиша без остановок"
                });
            }
            else
            {
                result.IsValid = false;
                result.FailureReason = $"Недостаточно топлива для поездки без остановок. " +
                    $"Нужно: {totalFuelNeeded + minReserve:F1}л, Есть: {context.CurrentFuelLiters:F1}л";
            }

            return result;
        }

        /// <summary>
        /// Проверяет упорядоченность станций по расстоянию
        /// </summary>
        private bool ValidateStationOrder(FuelChain chain, ChainValidationResult result)
        {
            for (int i = 1; i < chain.Stations.Count; i++)
            {
                if (chain.Stations[i].ForwardDistanceKm <= chain.Stations[i-1].ForwardDistanceKm)
                {
                    result.IsValid = false;
                    result.FailureReason = $"Станции не упорядочены по расстоянию: " +
                        $"{chain.Stations[i-1].Station?.ProviderName} ({chain.Stations[i-1].ForwardDistanceKm:F0}км) " +
                        $"после {chain.Stations[i].Station?.ProviderName} ({chain.Stations[i].ForwardDistanceKm:F0}км)";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Проверяет минимальные расстояния между станциями
        /// </summary>
        private bool ValidateMinimumDistances(FuelChain chain, FuelPlanningContext context, ChainValidationResult result)
        {
            var minDistance = FuelPlanningConfig.MinStopDistanceKm;
            var previousPosition = 0.0; // Старт

            foreach (var station in chain.Stations)
            {
                var distance = station.ForwardDistanceKm - previousPosition;
                
                // Для первой станции минимальное расстояние может быть меньше
                var requiredMinDistance = previousPosition == 0 ? 50.0 : minDistance;
                
                if (distance < requiredMinDistance)
                {
                    result.IsValid = false;
                    result.FailureReason = $"Нарушение минимального расстояния до станции " +
                        $"{station.Station?.ProviderName}: {distance:F0}км < {requiredMinDistance:F0}км";
                    return false;
                }

                previousPosition = station.ForwardDistanceKm;
            }

            return true;
        }

        /// <summary>
        /// Симулирует поездку и проверяет все ограничения
        /// </summary>
        private bool SimulateTripAndValidate(FuelChain chain, FuelPlanningContext context, ChainValidationResult result)
        {
            var currentFuel = context.CurrentFuelLiters;
            var currentPosition = 0.0;
            var minReserve = context.TankCapacity * FuelPlanningConfig.MinReserveFactor;
            var costCalculator = new SmartChainCostCalculator();

            bool isFirst = true;

            // Симулируем каждую остановку
            foreach (var station in chain.Stations)
            {
                var stepResult = new ValidationStepResult { Station = station };
                
                // Рассчитываем расход топлива до станции
                var distance = station.ForwardDistanceKm - currentPosition;
                var fuelUsed = distance * context.FuelConsumptionPerKm;
                var fuelAtArrival = currentFuel - fuelUsed;

                stepResult.FuelBefore = currentFuel;
                stepResult.Distance = distance;

                var fuelPercentAtArrival = (currentFuel / context.TankCapacity) * 100.0;

                // Проверка 1: Можем ли дойти до станции с 20% запасом

                if (!isFirst || (isFirst && fuelPercentAtArrival > 30) && fuelAtArrival < minReserve)
                {
                    result.IsValid = false;
                    result.FailureReason = $"Невозможно дойти до станции {station.Station?.ProviderName} " +
                        $"с соблюдением 20% запаса. При прибытии будет {fuelAtArrival:F1}л " +
                        $"< {minReserve:F1}л (20%)";
                    
                    stepResult.MeetsReserveRequirement = false;
                    stepResult.Notes = "НАРУШЕНИЕ 20% ЗАПАСА";
                    result.StepResults.Add(stepResult);
                    return false;
                }

                stepResult.MeetsReserveRequirement = true;

                // Рассчитываем оптимальную дозаправку
                var refillAmount = costCalculator.CalculateOptimalRefillAmount(
                    station, fuelAtArrival, chain.Stations.ToList(), context);

                if ((refillAmount / context.TankCapacity) * 100.0 < 15)
                {
                    result.IsValid = false;
                    return false;
                }

                // Проверка 2: Не превышаем ли вместимость бака
                if (fuelAtArrival + refillAmount > context.TankCapacity)
                {
                    result.Warnings.Add($"На станции {station.Station?.ProviderName} " +
                        $"дозаправка ограничена вместимостью бака");
                    refillAmount = context.TankCapacity - fuelAtArrival;
                }

                stepResult.RefillAmount = refillAmount;
                stepResult.FuelAfter = fuelAtArrival + refillAmount;
                stepResult.Notes = $"Успешная остановка, запас соблюден";

                result.StepResults.Add(stepResult);

                // Обновляем состояние для следующей итерации
                currentFuel = fuelAtArrival + refillAmount;
                currentPosition = station.ForwardDistanceKm;
            }

            // Проверяем финальный участок до финиша
            return ValidateFinishSegment(currentFuel, currentPosition, context, result);
        }

        /// <summary>
        /// Проверяет финальный участок до финиша
        /// </summary>
        private bool ValidateFinishSegment(
            double currentFuel, 
            double currentPosition, 
            FuelPlanningContext context,
            ChainValidationResult result)
        {
            var distanceToFinish = context.TotalDistanceKm - currentPosition;
            var fuelNeededToFinish = distanceToFinish * context.FuelConsumptionPerKm;
            var fuelAtFinish = currentFuel - fuelNeededToFinish;

            var finishStep = new ValidationStepResult
            {
                Station = null,
                FuelBefore = currentFuel,
                Distance = distanceToFinish,
                RefillAmount = 0,
                FuelAfter = fuelAtFinish
            };

            // Проверяем, хватит ли топлива до финиша с требуемым запасом
            if (fuelAtFinish < context.FinishFuel)
            {
                result.IsValid = false;
                result.FailureReason = $"Недостаточно топлива для достижения финиша. " +
                    $"На финише будет {fuelAtFinish:F1}л < требуемых {context.FinishFuel:F1}л";
                
                finishStep.MeetsReserveRequirement = false;
                finishStep.Notes = "НЕДОСТАТОЧНО ТОПЛИВА ДО ФИНИША";
                result.StepResults.Add(finishStep);
                return false;
            }

            finishStep.MeetsReserveRequirement = true;
            finishStep.Notes = "Успешное достижение финиша";
            result.StepResults.Add(finishStep);

            result.FinalFuelAmount = fuelAtFinish;
            return true;
        }
    }
}
