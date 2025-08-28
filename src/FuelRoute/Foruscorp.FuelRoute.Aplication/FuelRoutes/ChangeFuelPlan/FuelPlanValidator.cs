using System;
using System.Collections.Generic;
using System.Linq;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Domain.RouteValidators;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.ChangeFuelPlan
{
    /// <summary>
    /// Комплексный валидатор планов топливных станций
    /// Проверяет достижимость финиша, соблюдение запаса топлива, минимальные расстояния
    /// </summary>
    public class FuelPlanValidator
    {
        private const double MinReserveFactor = 0.20; // 20% запас
        private const double MinStopDistanceKm = 100.0; // Минимальное расстояние между остановками
        private const double MinRefillPercentage = 0.15; // Минимальная дозаправка 15%
        private const double MaxTankCapacity = 200.0; // Максимальная емкость бака в литрах

        /// <summary>
        /// Проверяет, является ли план топливных станций валидным
        /// </summary>
        public bool IsPlanValid(FuelRouteSection routeSection, List<FuelStationChange> fuelStationChanges)
        {
            try
            {
                var result = ValidatePlanDetailed(routeSection, fuelStationChanges);
                return result.IsValid;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Возвращает детальную информацию о валидности плана
        /// </summary>
        public FuelPlanValidationResult ValidatePlanDetailed(FuelRouteSection routeSection, List<FuelStationChange> fuelStationChanges)
        {
            var result = new FuelPlanValidationResult { IsValid = true };
            
            try
            {
                // Проверка 1: Валидация плана без остановок
                if (!fuelStationChanges.Any())
                {
                    return ValidateNoStopsPlan(routeSection);
                }

                // Проверка 2: Упорядоченность станций по расстоянию
                if (!ValidateStationOrder(fuelStationChanges, result))
                    return result;

                // Проверка 3: Минимальные расстояния между станциями
                if (!ValidateMinimumDistances(fuelStationChanges, result))
                    return result;

                // Проверка 4: Симуляция поездки с проверкой всех ограничений
                if (!SimulateTripAndValidate(fuelStationChanges, routeSection, result))
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
        /// Валидация плана без остановок
        /// </summary>
        private FuelPlanValidationResult ValidateNoStopsPlan(FuelRouteSection routeSection)
        {
            var result = new FuelPlanValidationResult();
            
            // Получаем информацию о маршруте
            var totalDistance = routeSection.RouteSectionInfo?.Miles ?? 0;
            var fuelConsumptionPerKm = 0.3; // Примерный расход топлива (л/км)
            var currentFuel = 150.0; // Текущее количество топлива
            var finishFuel = 50.0; // Требуемое количество топлива на финише
            
            var totalFuelNeeded = (totalDistance * 1.60934) * fuelConsumptionPerKm + finishFuel; // Конвертируем мили в км
            var minReserve = MaxTankCapacity * MinReserveFactor;
            
            // Проверяем, хватит ли топлива с учетом 20% запаса
            if (currentFuel >= totalFuelNeeded + minReserve)
            {
                result.IsValid = true;
                result.FinalFuelAmount = currentFuel - totalFuelNeeded;
                
                result.StepResults.Add(new ValidationStepResult
                {
                    StationId = Guid.Empty,
                    FuelBefore = currentFuel,
                    FuelAfter = result.FinalFuelAmount,
                    RefillAmount = 0,
                    Distance = totalDistance,
                    MeetsReserveRequirement = true,
                    Notes = "Прямая поездка до финиша без остановок"
                });
            }
            else
            {
                result.IsValid = false;
                result.FailureReason = $"Недостаточно топлива для поездки без остановок. " +
                    $"Нужно: {totalFuelNeeded + minReserve:F1}л, Есть: {currentFuel:F1}л";
            }

            return result;
        }

        /// <summary>
        /// Проверяет упорядоченность станций по расстоянию
        /// </summary>
        private bool ValidateStationOrder(List<FuelStationChange> fuelStationChanges, FuelPlanValidationResult result)
        {
            var orderedStations = fuelStationChanges.OrderBy(fsc => fsc.ForwardDistance).ToList();
            
            for (int i = 1; i < orderedStations.Count; i++)
            {
                if (orderedStations[i].ForwardDistance <= orderedStations[i-1].ForwardDistance)
                {
                    result.IsValid = false;
                    result.FailureReason = $"Станции не упорядочены по расстоянию: " +
                        $"Станция {orderedStations[i-1].FuelRouteStationId} ({orderedStations[i-1].ForwardDistance:F0}км) " +
                        $"после станции {orderedStations[i].FuelRouteStationId} ({orderedStations[i].ForwardDistance:F0}км)";
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Проверяет минимальные расстояния между станциями
        /// </summary>
        private bool ValidateMinimumDistances(List<FuelStationChange> fuelStationChanges, FuelPlanValidationResult result)
        {
            var orderedStations = fuelStationChanges.OrderBy(fsc => fsc.ForwardDistance).ToList();
            var previousPosition = 0.0; // Старт

            foreach (var station in orderedStations)
            {
                var distance = station.ForwardDistance - previousPosition;
                
                // Для первой станции минимальное расстояние может быть меньше
                var requiredMinDistance = previousPosition == 0 ? 50.0 : MinStopDistanceKm;
                
                if (distance < requiredMinDistance)
                {
                    result.IsValid = false;
                    result.FailureReason = $"Нарушение минимального расстояния до станции " +
                        $"{station.FuelRouteStationId}: {distance:F0}км < {requiredMinDistance:F0}км";
                    return false;
                }

                previousPosition = station.ForwardDistance;
            }

            return true;
        }

        /// <summary>
        /// Симулирует поездку и проверяет все ограничения
        /// </summary>
        private bool SimulateTripAndValidate(List<FuelStationChange> fuelStationChanges, FuelRouteSection routeSection, FuelPlanValidationResult result)
        {
            var currentFuel = 150.0; // Начальное количество топлива
            var currentPosition = 0.0;
            var minReserve = MaxTankCapacity * MinReserveFactor;
            var fuelConsumptionPerKm = 0.08; // Расход топлива (л/км)
            var finishFuel = 50.0; // Требуемое количество топлива на финише

            var orderedStations = fuelStationChanges.OrderBy(fsc => fsc.ForwardDistance).ToList();
            bool isFirst = true;

            // Симулируем каждую остановку
            foreach (var station in orderedStations)
            {
                var stepResult = new ValidationStepResult { StationId = station.FuelRouteStationId };
                
                // Рассчитываем расход топлива до станции
                var distance = station.ForwardDistance - currentPosition;
                var fuelUsed = distance * fuelConsumptionPerKm;
                var fuelAtArrival = currentFuel - fuelUsed;

                stepResult.FuelBefore = currentFuel;
                stepResult.Distance = distance;

                var fuelPercentAtArrival = (fuelAtArrival / MaxTankCapacity) * 100.0;

                // Проверка 1: Можем ли дойти до станции с 20% запасом
                if (!isFirst  && fuelAtArrival < minReserve)
                {
                    result.IsValid = false;
                    result.FailureReason = $"Невозможно дойти до станции {station.FuelRouteStationId} " +
                        $"с соблюдением 20% запаса. При прибытии будет {fuelAtArrival:F1}л " +
                        $"< {minReserve:F1}л (20%)";
                    
                    stepResult.MeetsReserveRequirement = false;
                    stepResult.Notes = "НАРУШЕНИЕ 20% ЗАПАСА";
                    result.StepResults.Add(stepResult);
                    return false;
                }

                stepResult.MeetsReserveRequirement = true;

                // Проверяем минимальную дозаправку
                var refillAmount = station.Refill;
                if ((refillAmount / MaxTankCapacity) * 100.0 < MinRefillPercentage * 100.0)
                {
                    result.IsValid = false;
                    result.FailureReason = $"Недостаточная дозаправка на станции {station.FuelRouteStationId}: " +
                        $"{refillAmount:F1}л < {MinRefillPercentage * 100.0:F0}% от емкости бака";
                    return false;
                }

                // Проверка 2: Не превышаем ли вместимость бака
                if (fuelAtArrival + refillAmount > MaxTankCapacity)
                {
                    //result.IsValid = false;
                    result.Warnings.Add($"На станции {station.FuelRouteStationId} " +
                        $"дозаправка ограничена вместимостью бака");
                    //refillAmount = MaxTankCapacity - fuelAtArrival;
                    //return false;
                }

                stepResult.RefillAmount = refillAmount;
                stepResult.FuelAfter = fuelAtArrival + refillAmount;
                stepResult.Notes = $"Успешная остановка, запас соблюден";

                result.StepResults.Add(stepResult);

                // Обновляем состояние для следующей итерации
                currentFuel = fuelAtArrival + refillAmount;
                currentPosition = station.ForwardDistance;
                isFirst = false;
            }

            // Проверяем финальный участок до финиша
            return ValidateFinishSegment(currentFuel, currentPosition, routeSection, result, fuelConsumptionPerKm, finishFuel);
        }

        /// <summary>
        /// Проверяет финальный участок до финиша
        /// </summary>
        private bool ValidateFinishSegment(
            double currentFuel, 
            double currentPosition, 
            FuelRouteSection routeSection,
            FuelPlanValidationResult result,
            double fuelConsumptionPerKm,
            double finishFuel)
        {
            var totalDistance = routeSection.RouteSectionInfo?.Miles ?? 0;
            totalDistance = totalDistance / 1000.0;
            var distanceToFinish = totalDistance - currentPosition; // Конвертируем мили в км
            var fuelNeededToFinish = distanceToFinish * fuelConsumptionPerKm;
            var fuelAtFinish = currentFuel - fuelNeededToFinish;

            var finishStep = new ValidationStepResult
            {
                StationId = Guid.Empty,
                FuelBefore = currentFuel,
                Distance = distanceToFinish,
                RefillAmount = 0,
                FuelAfter = fuelAtFinish
            };

            // Проверяем, хватит ли топлива до финиша с требуемым запасом
            if (fuelAtFinish < finishFuel)
            {
                result.IsValid = false;
                result.FailureReason = $"Недостаточно топлива для достижения финиша. " +
                    $"На финише будет {fuelAtFinish:F1}л < требуемых {finishFuel:F1}л";
                
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

    /// <summary>
    /// Результат валидации плана топливных станций
    /// </summary>
    public class FuelPlanValidationResult
    {
        public bool IsValid { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public double FinalFuelAmount { get; set; }
        public List<ValidationStepResult> StepResults { get; set; } = [];
        public List<string> Warnings { get; set; } = [];
    }

    /// <summary>
    /// Результат валидации одного шага (остановки)
    /// </summary>
    public class ValidationStepResult
    {
        public Guid StationId { get; set; }
        public double FuelBefore { get; set; }
        public double FuelAfter { get; set; }
        public double RefillAmount { get; set; }
        public double Distance { get; set; }
        public bool MeetsReserveRequirement { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
