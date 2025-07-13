using Foruscorp.TrucksTracking.Worker.Services;

namespace Foruscorp.TrucksTracking.Worker.Contauct
{
    public interface ITruckProviderService
    {
        Task<VehicleResponse> GetVehiclesAsync(CancellationToken cancellationToken = default);
        Task<VehicleStatsResponse> GetVehicleLocationsFeedAsync(string after = null, CancellationToken cancellationToken = default);
        Task<VehicleStatsResponse> GetVehicleStatsFeedAsync(List<string> vehicleId = null, DateTime historiTimeSpun = default, CancellationToken cancellationToken = default);
        Task<VehicleStatsResponse> GetHistoricalStatsAsync(List<string> vehicleIds = null, DateTime historiTimeSpun = default, CancellationToken cancellationToken = default);
    }
}
