using System.Threading;
using Foruscorp.Trucks.Aplication.Contruct.Samasara;

namespace Foruscorp.Trucks.Aplication.Contruct
{
    public interface ITruckProviderService
    {
        Task<VehicleResponse> GetVehiclesAsync(CancellationToken cancellationToken = default);
        Task<VehicleStatsResponse> GetVehicleLocationsFeedAsync(string after = null, CancellationToken cancellationToken = default);
        Task<VehicleStatsResponse> GetVehicleStatsFeedAsync(string vehicleId = null, string after = null, CancellationToken cancellationToken = default);
    }
}
