using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Domain.Trucks
{
    public interface ITruckRepository
    {
        Task<TruckTracker> GetTruckById(Guid truckId);
        Task AddTruck(TruckTracker truck, CancellationToken cancellationToken);
        Task UpdateTruck(TruckTracker truck, CancellationToken cancellationToken);
        Task DeleteTruck(Guid truckId, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
