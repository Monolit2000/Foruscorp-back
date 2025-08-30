using System;
using System.Collections.Generic;
using System.Linq;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Domain.RouteValidators;
using Org.BouncyCastle.Asn1.Crmf;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.ChangeFuelPlan
{
    /// <summary>
    /// Калькулятор расхода топлива на основе веса груза
    /// </summary>
    public class FuelConsumptionCalculator
    {
        private const double ReferenceWeightLb = 40000.0;
        private const double ReferenceConsumptionGPerKm = 0.089;

        public FuelParameters CalculateFuelParameters(double weightLb, double currentFuelPercent)
        {
            var tankCapacityG = 200.0;
            var initialFuelG = tankCapacityG * (currentFuelPercent / 100.0);

            return new FuelParameters
            {
                ConsumptionGPerKm = CalculateConsumptionGPerKm(weightLb),
                TankCapacityG = tankCapacityG,
                InitialFuelG = initialFuelG,
                CurrentFuelPercent = currentFuelPercent
            };
        }

        private static double CalculateConsumptionGPerKm(double weightLb)
        {
            return ReferenceConsumptionGPerKm * (weightLb / ReferenceWeightLb);
        }
    }

    /// <summary>
    /// Параметры топлива для расчета
    /// </summary>
    public class FuelParameters
    {
        public double ConsumptionGPerKm { get; set; }
        public double TankCapacityG { get; set; }
        public double InitialFuelG { get; set; }
        public double CurrentFuelPercent { get; set; }
    }

    /// <summary>
    /// Контекст планирования топлива
    /// </summary>
    public class FuelPlanningContext
    {
        public double TotalDistanceKm { get; set; }
        public double FuelConsumptionPerKm { get; set; }
        public double CurrentFuelLiters { get; set; }
        public double TankCapacity { get; set; }
        public double FinishFuel { get; set; }
        public double WeightLb { get; set; }
    }

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
        public bool IsPlanValid(FuelRouteSection routeSection, List<FuelStationChange> fuelStationChanges, double weightLb = 40000.0, double currentFuelPercent = 100.0, double maxTankCapacity = 200.0, double minStopDistanceKm = 100.0, double minReserveFactor = 0.20, double fuelFinish = 50.0)
        {
            try
            {
                var result = ValidatePlanDetailed(routeSection, fuelStationChanges, weightLb, currentFuelPercent, maxTankCapacity, minStopDistanceKm, minReserveFactor, fuelFinish);
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
        public FuelPlanValidationResult ValidatePlanDetailed(FuelRouteSection routeSection, List<FuelStationChange> fuelStationChanges, double weightLb = 40000.0, double currentFuelPercent = 100.0, double maxTankCapacity = 200.0, double minStopDistanceKm = 100.0, double minReserveFactor = 0.20, double fuelFinish = 50.0)
        {
            var result = new FuelPlanValidationResult { IsValid = true };
            
            try
            {
                // Инициализируем калькулятор расхода топлива
                var fuelCalculator = new FuelConsumptionCalculator();
                var fuelParams = fuelCalculator.CalculateFuelParameters(weightLb, currentFuelPercent);

                // Проверка 1: Валидация плана без остановок
                if (!fuelStationChanges.Any())
                {
                    return ValidateNoStopsPlan(routeSection, fuelParams, maxTankCapacity, minReserveFactor, fuelFinish);
                }

                // Проверка 2: Упорядоченность станций по расстоянию
                if (!ValidateStationOrder(fuelStationChanges, result))
                    return result;

                //// Проверка 3: Минимальные расстояния между станциями
                //if (!ValidateMinimumDistances(fuelStationChanges, result, minStopDistanceKm))
                //    return result;

                // Проверка 4: Симуляция поездки с проверкой всех ограничений
                if (!SimulateTripAndValidate(fuelStationChanges, routeSection, result, fuelParams, maxTankCapacity, minReserveFactor, fuelFinish))
                    return result;

                // Если все проверки пройдены
                result.IsValid = true;
                return result;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                                 result.FailureReason = $"Validation error: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Валидация плана без остановок
        /// </summary>
        private FuelPlanValidationResult ValidateNoStopsPlan(FuelRouteSection routeSection, FuelParameters fuelParams, double maxTankCapacity, double minReserveFactor, double fuelFinish)
        {
            var result = new FuelPlanValidationResult();
            
            // Получаем информацию о маршруте
            var totalDistance = routeSection.RouteSectionInfo?.Miles ?? 0;
            var totalDistanceKm = totalDistance / 1000; // Конвертируем мили в км
            
            // Используем точный расчет расхода топлива
            var fuelConsumptionPerKm = fuelParams.ConsumptionGPerKm;
            var currentFuel = fuelParams.InitialFuelG;
            
            var totalFuelNeeded = totalDistanceKm * fuelConsumptionPerKm + fuelFinish;
            var minReserve = maxTankCapacity * minReserveFactor;
            
            // Проверяем, хватит ли топлива с учетом запаса
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
                     Distance = totalDistanceKm,
                     MeetsReserveRequirement = true,
                     Notes = $"Direct trip to finish without stops. Consumption: {fuelConsumptionPerKm:F3} g/km"
                 });
                 
            }
            else
            {
                                 result.IsValid = false;
                 result.FailureReason = $"Insufficient fuel for trip without stops. " +
                     $"Required: {totalFuelNeeded + minReserve:F1}g, Available: {currentFuel:F1}g. " +
                     $"Consumption: {fuelConsumptionPerKm:F3} g/km";
            }

            return result;
        }

                 /// <summary>
         /// Проверяет упорядоченность станций по расстоянию
         /// </summary>
         private bool ValidateStationOrder(List<FuelStationChange> fuelStationChanges, FuelPlanValidationResult result)
         {
             var orderedStations = fuelStationChanges.OrderBy(fsc => fsc.ForwardDistance).ToList();
             
             // Проставляем StopOrder для всех станций
             for (int i = 0; i < orderedStations.Count; i++)
             {
                 orderedStations[i].StopOrder = i + 1;
             }
             
             for (int i = 1; i < orderedStations.Count; i++)
             {
                 if (orderedStations[i].ForwardDistance <= orderedStations[i-1].ForwardDistance)
                 {
                      var stepResult = new ValidationStepResult
                      {
                          IsValid = false,
                          StationId = orderedStations[i].Id,
                          StationsNotOrdered = true,
                          PreviousStationDistance = orderedStations[i-1].ForwardDistance,
                          CurrentStationDistance = orderedStations[i].ForwardDistance,
                          Notes = $"Stations are not ordered by distance"
                      };
                     
                     result.StepResults.Add(stepResult);
                                          result.IsValid = false;
                      result.FailureReason = $"Stations are not ordered by distance: " +
                          $"Station {orderedStations[i-1].FuelRouteStationId} ({orderedStations[i-1].ForwardDistance:F0}km) " +
                          $"after station {orderedStations[i].FuelRouteStationId} ({orderedStations[i].ForwardDistance:F0}km)";
                     return false;
                 }
             }
             return true;
         }

                 /// <summary>
         /// Проверяет минимальные расстояния между станциями
         /// </summary>
         private bool ValidateMinimumDistances(List<FuelStationChange> fuelStationChanges, FuelPlanValidationResult result, double minStopDistanceKm)
         {
             var orderedStations = fuelStationChanges.OrderBy(fsc => fsc.ForwardDistance).ToList();
             var previousPosition = 0.0; // Старт

             // Проставляем StopOrder для всех станций
             for (int i = 0; i < orderedStations.Count; i++)
             {
                 orderedStations[i].StopOrder = i + 1;
             }

             foreach (var station in orderedStations)
             {
                 var distance = station.ForwardDistance - previousPosition;
                 
                 // Для первой станции минимальное расстояние может быть меньше
                 var requiredMinDistance = previousPosition == 0 ? 50.0 : minStopDistanceKm;
                 
                 if (distance < requiredMinDistance)
                 {
                      var stepResult = new ValidationStepResult
                      {
                          IsValid = false,
                          StationId = station.Id,
                          DistanceTooClose = true,
                          MinRequiredDistance = requiredMinDistance,
                          ActualDistance = distance,
                          Distance = distance,
                          Notes = $"Minimum distance violation to station. \n ActualDistance: {distance * 0.621371}"
                      };
                     
                     result.StepResults.Add(stepResult);
                                          result.IsValid = false;
                      result.FailureReason = $"Minimum distance violation to station " +
                          $"{station.FuelRouteStationId}: {distance:F0}km < {requiredMinDistance:F0}km";
                     return false;
                 }

                 previousPosition = station.ForwardDistance;
             }

             return true;
         }

        /// <summary>
        /// Симулирует поездку и проверяет все ограничения
        /// </summary>
        private bool SimulateTripAndValidate(List<FuelStationChange> fuelStationChanges, FuelRouteSection routeSection, FuelPlanValidationResult result, FuelParameters fuelParams, double maxTankCapacity, double minReserveFactor, double fuelFinish)
        {
            var currentFuel = fuelParams.InitialFuelG; // Начальное количество топлива
            var currentPosition = 0.0;
            var minReserve = maxTankCapacity * minReserveFactor;
            var fuelConsumptionPerKm = fuelParams.ConsumptionGPerKm; // Точный расход топлива (г/км)

            var orderedStations = fuelStationChanges.OrderBy(fsc => fsc.ForwardDistance).ToList();
            bool isFirst = true;

                         // Симулируем каждую остановку
             for (int i = 0; i < orderedStations.Count; i++)
             {
                 var station = orderedStations[i];
                 var stepResult = new ValidationStepResult { StationId = station.Id };
                 
                 // Рассчитываем расход топлива до станции
                 var distance = station.ForwardDistance - currentPosition;
                 var fuelUsed = distance * fuelConsumptionPerKm;
                 var fuelAtArrival = currentFuel - fuelUsed;

                 // Проставляем StopOrder для FuelStationChange (начиная с 1)
                 station.StopOrder = i + 1;
                 
                 // Проставляем CurrentFuel для FuelStationChange
                 station.CurrentFuel = fuelAtArrival;
                 
                 // Проставляем NextDistanceKm для FuelStationChange
                 if (i < orderedStations.Count - 1)
                 {
                     // Расстояние до следующей станции
                     var nextStation = orderedStations[i + 1];
                     station.NextDistanceKm = nextStation.ForwardDistance - station.ForwardDistance;
                 }
                 else
                 {
                     // Для последней станции - расстояние до финиша
                     var totalDistance = routeSection.RouteSectionInfo?.Miles ?? 0;
                     var totalDistanceKm = totalDistance / 1000.0;
                     station.NextDistanceKm = totalDistanceKm - station.ForwardDistance;
                 }

                 stepResult.FuelBefore = currentFuel;
                 stepResult.Distance = distance;

                var fuelPercentAtArrival = (fuelAtArrival / maxTankCapacity) * 100.0;

                // Проверка 1: Можем ли дойти до станции с требуемым запасом
                if (!isFirst && fuelAtArrival < minReserve)
                {
                    stepResult.IsValid = false;
                    stepResult.InsufficientFuelReserve = true;
                    stepResult.RequiredFuelReserve = minReserve;
                    stepResult.ActualFuelReserve = fuelAtArrival;
                    stepResult.MeetsReserveRequirement = false;
                                         stepResult.Notes = $"VIOLATION OF 20% RESERVE";
                    
                    result.StepResults.Add(stepResult);
                    result.IsValid = false;
                                         result.FailureReason = $"Cannot reach station {station.FuelRouteStationId} " +
                         $"with {minReserveFactor * 100:F0}% reserve. Arrival fuel will be {fuelAtArrival:F1}g " +
                         $"< {minReserve:F1}g ({minReserveFactor * 100:F0}%)";
                    return false;
                }

                stepResult.MeetsReserveRequirement = true;

                // Проверяем минимальную дозаправку
                var refillAmount = station.Refill;
                if ((refillAmount / maxTankCapacity) * 100.0 < MinRefillPercentage * 100.0)
                {
                    stepResult.IsValid = false;
                    stepResult.InsufficientRefillAmount = true;
                    stepResult.RequiredRefillAmount = maxTankCapacity * MinRefillPercentage;
                    stepResult.ActualRefillAmount = refillAmount;
                    stepResult.MaxTankCapacity = maxTankCapacity;
                                         stepResult.Notes = $"Insufficient refill at station. ActualRefillAmount: \n {stepResult.ActualRefillAmount}";
                    
                    result.StepResults.Add(stepResult);
                    result.IsValid = false;
                                         result.FailureReason = $"Insufficient refill at station {station.FuelRouteStationId}: " +
                         $"{refillAmount:F1}g < {MinRefillPercentage * 100.0:F0}% of tank capacity";
                    
                    return false;
                }

                // Проверка 2: Не превышаем ли вместимость бака
                if (fuelAtArrival + refillAmount > maxTankCapacity)
                {
                    stepResult.IsValid = false;
                    stepResult.TankCapacityExceeded = true;
                    stepResult.MaxTankCapacity = maxTankCapacity;
                    stepResult.AttemptedRefill = fuelAtArrival + refillAmount;
                    stepResult.FuelBefore = fuelAtArrival;
                                         stepResult.Notes = $"Tank capacity exceeded at station. \n AttemptedRefill: {stepResult.AttemptedRefill} gl";
                    
                    result.StepResults.Add(stepResult);
                    result.IsValid = false;
                                         result.Warnings.Add($"At station {station.FuelRouteStationId} " +
                         $"refill is limited by tank capacity ({maxTankCapacity:F1}g)");
                    refillAmount = maxTankCapacity - fuelAtArrival;
                    return false;
                }

                stepResult.RefillAmount = refillAmount;
                stepResult.FuelAfter = fuelAtArrival + refillAmount;
                                 stepResult.Notes = $"Successful stop, reserve maintained";

                result.StepResults.Add(stepResult);

                // Обновляем состояние для следующей итерации
                currentFuel = fuelAtArrival + refillAmount;
                currentPosition = station.ForwardDistance;
                isFirst = false;
            }

            // Проверяем финальный участок до финиша
            return ValidateFinishSegment(currentFuel, currentPosition, routeSection, result, fuelConsumptionPerKm, fuelFinish, fuelParams);
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
            double fuelFinish,
            FuelParameters fuelParams)
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
             
             // Для финального шага CurrentFuel будет равно fuelAtFinish
             // Но поскольку это не FuelStationChange, это не применимо

            // Проверяем, хватит ли топлива до финиша с требуемым запасом
            if (fuelAtFinish < fuelFinish)
            {
                finishStep.IsValid = false;
                finishStep.InsufficientFuelToFinish = true;
                finishStep.RequiredFuelToFinish = fuelFinish;
                finishStep.ActualFuelAtFinish = fuelAtFinish;
                finishStep.MeetsReserveRequirement = false;
                                 finishStep.Notes = "INSUFFICIENT FUEL TO REACH FINISH";
                
                result.StepResults.Add(finishStep);
                result.IsValid = false;
                                 result.FailureReason = $"Insufficient fuel to reach finish. " +
                     $"At finish will be {fuelAtFinish:F1}g < required {fuelFinish:F1}g. " +
                     $"Consumption: {fuelConsumptionPerKm:F3} g/km";
                
                return false;
            }

            finishStep.MeetsReserveRequirement = true;
                         finishStep.Notes = "Successfully reached finish";
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

        public bool IsValid { get; set; } = true;


        // Плохие случаи валидации
        public bool InsufficientFuelReserve { get; set; } = false;
        public bool InsufficientRefillAmount { get; set; } = false;
        public bool TankCapacityExceeded { get; set; } = false;
        public bool InsufficientFuelToFinish { get; set; } = false;
        public bool DistanceTooClose { get; set; } = false;
        public bool StationsNotOrdered { get; set; } = false;
        
        // Детали плохих случаев
        public double RequiredFuelReserve { get; set; } = 0.0;
        public double ActualFuelReserve { get; set; } = 0.0;
        public double RequiredRefillAmount { get; set; } = 0.0;
        public double ActualRefillAmount { get; set; } = 0.0;
        public double MaxTankCapacity { get; set; } = 0.0;
        public double AttemptedRefill { get; set; } = 0.0;
        public double RequiredFuelToFinish { get; set; } = 0.0;
        public double ActualFuelAtFinish { get; set; } = 0.0;
        public double MinRequiredDistance { get; set; } = 0.0;
        public double ActualDistance { get; set; } = 0.0;
        public double PreviousStationDistance { get; set; } = 0.0;
        public double CurrentStationDistance { get; set; } = 0.0;
    }
}
