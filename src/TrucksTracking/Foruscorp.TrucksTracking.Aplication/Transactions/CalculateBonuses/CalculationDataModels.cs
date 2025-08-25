using Foruscorp.TrucksTracking.Aplication.Contruct.ApiClients;
using Foruscorp.TrucksTracking.Domain.FuelStationPlans;
using Foruscorp.TrucksTracking.Domain.Transactions;
using System;
using System.Collections.Generic;

namespace Foruscorp.TrucksTracking.Aplication.Transactions.CalculateBonuses
{
    /// <summary>
    /// Базовые данные для расчета бонусов из локальной базы данных
    /// </summary>
    public class BaseCalculationData
    {
        public List<NearFuelStationPlan> NearStationPlans { get; set; } = new();
        public List<Transaction> Transactions { get; set; } = new();
        public List<Guid> TruckIds { get; set; } = new();
        public List<Guid> FuelStationIds { get; set; } = new();
    }

    /// <summary>
    /// Детальные данные для расчета бонусов из всех микросервисов
    /// </summary>
    public class DetailedCalculationData
    {
        public BaseCalculationData BaseData { get; set; } = new();
        public Dictionary<Guid, TruckInfoDto> TrucksInfo { get; set; } = new();
        public Dictionary<Guid, FuelStationInfoDto> StationsInfo { get; set; } = new();
        public Dictionary<Guid, DriverInfoDto> DriversInfo { get; set; } = new();
        public Dictionary<Guid, CompanyInfoDto> CompaniesInfo { get; set; } = new();
        public Dictionary<Guid, List<FuelPriceDto>> StationPrices { get; set; } = new();
        public Dictionary<Guid, FuelStationRatingDto> StationRatings { get; set; } = new();
        public Dictionary<Guid, TruckEfficiencyDto> TruckEfficiencies { get; set; } = new();
    }

    /// <summary>
    /// Промежуточные данные для расчета бонусов конкретного грузовика
    /// </summary>
    public class TruckCalculationContext
    {
        public Guid TruckId { get; set; }
        public TruckInfoDto TruckInfo { get; set; } = new();
        public DriverInfoDto? DriverInfo { get; set; }
        public CompanyInfoDto CompanyInfo { get; set; } = new();
        public TruckEfficiencyDto? EfficiencyInfo { get; set; }
        public List<Transaction> TruckTransactions { get; set; } = new();
        public List<NearFuelStationPlan> NearStationPlans { get; set; } = new();
        public Dictionary<Guid, FuelStationInfoDto> UsedStations { get; set; } = new();
        public Dictionary<Guid, List<FuelPriceDto>> StationPrices { get; set; } = new();
        public Dictionary<Guid, FuelStationRatingDto> StationRatings { get; set; } = new();
    }
}
