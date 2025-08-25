using System;
using System.Collections.Generic;

namespace Foruscorp.TrucksTracking.Aplication.Transactions.CalculateBonuses
{
    public class BonusCalculationResult
    {
        public int TotalTransactionsProcessed { get; set; }
        public int TrucksProcessed { get; set; }
        public int FuelStationsInvolved { get; set; }
        public decimal TotalBonusAmount { get; set; }

        public List<TruckBonusDetail> TruckBonusDetails { get; set; } = new();
        public DateTime CalculationStartTime { get; set; }
        public DateTime CalculationEndTime { get; set; }
        public TimeSpan CalculationDuration => CalculationEndTime - CalculationStartTime;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public BonusTypeStatistics BonusStatistics { get; set; } = new();
    }

    public class TruckBonusDetail
    {
        public Guid TruckId { get; set; }
        public string TruckNumber { get; set; } = string.Empty;
        public Guid? DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal TotalBonusAmount { get; set; }
        public int TransactionCount { get; set; }
        public double TotalFuelAmount { get; set; }
        public decimal TotalFuelCost { get; set; }
        public List<BonusTypeDetail> BonusByType { get; set; } = new();
        public double EfficiencyScore { get; set; }
        public double SavingsPercentage { get; set; }
    }

    public class BonusTypeDetail
    {
        public string BonusType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CalculationBasis { get; set; } = string.Empty;
    }

    public class BonusTypeStatistics
    {
        public decimal EfficiencyBonuses { get; set; }
        public decimal PriceSavingBonuses { get; set; }
        public decimal RouteComplianceBonuses { get; set; }
        public decimal PartnerStationBonuses { get; set; }
        public decimal VolumeBonuses { get; set; }
        public decimal Penalties { get; set; }
    }
}
