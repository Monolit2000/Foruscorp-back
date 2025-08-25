using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct.ApiClients
{
    /// <summary>
    /// Клиент для работы с FuelStations API
    /// </summary>
    public interface IFuelStationsApiClient
    {
        /// <summary>
        /// Получить информацию о топливной станции
        /// </summary>
        Task<FuelStationInfoDto> GetFuelStationInfoAsync(Guid fuelStationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить список топливных станций по ID
        /// </summary>
        Task<List<FuelStationInfoDto>> GetFuelStationsInfoAsync(List<Guid> fuelStationIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить цены на топливо на станции
        /// </summary>
        Task<List<FuelPriceDto>> GetFuelPricesAsync(Guid fuelStationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить станции рядом с координатами
        /// </summary>
        Task<List<FuelStationInfoDto>> GetNearbyStationsAsync(double latitude, double longitude, double radiusKm = 50, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить рейтинг станции
        /// </summary>
        Task<FuelStationRatingDto> GetStationRatingAsync(Guid fuelStationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Получить историю цен на станции
        /// </summary>
        Task<List<FuelPriceHistoryDto>> GetPriceHistoryAsync(Guid fuelStationId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    }

    #region DTOs

    public class FuelStationInfoDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool Has24HourAccess { get; set; }
        public bool HasTruckParking { get; set; }
        public bool HasRestaurant { get; set; }
        public bool HasShower { get; set; }
        public bool HasWifi { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class FuelPriceDto
    {
        public Guid Id { get; set; }
        public Guid FuelStationId { get; set; }
        public string FuelType { get; set; } = string.Empty; // Diesel, Gasoline, etc.
        public double Price { get; set; }
        public string Currency { get; set; } = "USD";
        public DateTime PriceDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class FuelStationRatingDto
    {
        public Guid FuelStationId { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public double ServiceRating { get; set; }
        public double CleanlinessRating { get; set; }
        public double PriceRating { get; set; }
        public double AccessibilityRating { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class FuelPriceHistoryDto
    {
        public Guid Id { get; set; }
        public Guid FuelStationId { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public double Price { get; set; }
        public DateTime PriceDate { get; set; }
        public double ChangeFromPrevious { get; set; }
        public double PercentChangeFromPrevious { get; set; }
    }

    #endregion
}
