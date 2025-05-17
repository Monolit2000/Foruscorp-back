using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.TrucksTracking.Aplication.Contruct.TruckProvider;

namespace Foruscorp.TrucksTracking.Aplication.Contruct
{
    public interface ITruckProviderService
    {
        Task<VehicleResponse> GetVehiclesAsync(CancellationToken cancellationToken = default);
        Task<VehicleStatsResponse> GetVehicleLocationsFeedAsync(string after = null, CancellationToken cancellationToken = default);
        Task<VehicleStatsResponse> GetVehicleStatsFeedAsync(List<string> vehicleId = null, DateTime historiTimeSpun = default, CancellationToken cancellationToken = default);
        Task<VehicleStatsResponse> GetHistoricalStatsAsync(List<string> vehicleIds = null, DateTime historiTimeSpun = default, CancellationToken cancellationToken = default);
    }
}
