using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct.ApiClients
{
    /// <summary>
    /// Клиент для работы с Trucks API
    /// </summary>
    public interface ITrucksApiClient
    {
        /// <summary>
        /// Получить информацию о грузовике
        /// </summary>
        Task<TruckInfoDto> GetTruckInfoAsync(Guid truckId, CancellationToken cancellationToken = default);

        Task<DriverInfoDto> GetDriverInfoAsync(Guid driverId, CancellationToken cancellationToken = default);

        Task<CompanyInfoDto> GetCompanyInfoAsync(Guid companyId, CancellationToken cancellationToken = default);

        Task<List<TruckInfoDto>> GetTrucksInfoAsync(List<Guid> truckIds, CancellationToken cancellationToken = default);

        Task<List<FuelHistoryDto>> GetTruckFuelHistoryAsync(Guid truckId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);

        Task<TruckEfficiencyDto> GetTruckEfficiencyAsync(Guid truckId, CancellationToken cancellationToken = default);
    }

    #region DTOs

    public class TruckInfoDto
    {
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public Guid? DriverId { get; set; }
        public Guid CompanyId { get; set; }
        public double FuelCapacity { get; set; }
        public double CurrentFuelLevel { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DriverInfoDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CompanyInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class FuelHistoryDto
    {
        public Guid Id { get; set; }
        public Guid TruckId { get; set; }
        public DateTime DateTime { get; set; }
        public double FuelAdded { get; set; }
        public double PricePerGallon { get; set; }
        public double TotalCost { get; set; }
        public string Location { get; set; } = string.Empty;
        public double Odometer { get; set; }
    }

    public class TruckEfficiencyDto
    {
        public Guid TruckId { get; set; }
        public double AverageMilesPerGallon { get; set; }
        public double TotalMilesDriven { get; set; }
        public double TotalFuelConsumed { get; set; }
        public double EfficiencyScore { get; set; }
        public DateTime CalculatedAt { get; set; }
    }

    #endregion
}
