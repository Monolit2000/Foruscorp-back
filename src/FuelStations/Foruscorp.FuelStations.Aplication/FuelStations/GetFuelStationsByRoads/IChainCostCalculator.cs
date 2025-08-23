using System.Collections.Generic;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    /// <summary>
    /// Интерфейс для расчета стоимости цепочки заправок
    /// </summary>
    public interface IChainCostCalculator
    {
        /// <summary>
        /// Рассчитывает полную стоимость цепочки заправок
        /// </summary>
        ChainCost CalculateChainCost(FuelChain chain, FuelPlanningContext context);

        /// <summary>
        /// Рассчитывает оптимальное количество дозаправки на конкретной станции
        /// </summary>
        double CalculateOptimalRefillAmount(
            StationInfo station, 
            double fuelAtArrival, 
            List<StationInfo> remainingStations, 
            FuelPlanningContext context);
    }

    /// <summary>
    /// Интерфейс для валидации цепочек заправок
    /// </summary>
    public interface IChainValidator
    {
        /// <summary>
        /// Проверяет, является ли цепочка валидной (можно ли с ней доехать до финиша)
        /// </summary>
        bool IsChainValid(FuelChain chain, FuelPlanningContext context);

        /// <summary>
        /// Возвращает детальную информацию о валидности цепочки
        /// </summary>
        ChainValidationResult ValidateChainDetailed(FuelChain chain, FuelPlanningContext context);
    }

    /// <summary>
    /// Результат валидации цепочки
    /// </summary>
    public class ChainValidationResult
    {
        public bool IsValid { get; set; }
        public string FailureReason { get; set; } = string.Empty;
        public List<string> Warnings { get; set; } = new List<string>();
        public double FinalFuelAmount { get; set; }
        public List<ValidationStepResult> StepResults { get; set; } = new List<ValidationStepResult>();
    }

    /// <summary>
    /// Результат валидации отдельного шага
    /// </summary>
    public class ValidationStepResult
    {
        public StationInfo? Station { get; set; }
        public double FuelBefore { get; set; }
        public double FuelAfter { get; set; }
        public double RefillAmount { get; set; }
        public double Distance { get; set; }
        public bool MeetsReserveRequirement { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
