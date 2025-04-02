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
        Task<Truck> GetTruckById(Guid truckId);
        Task AddTruck(Truck truck, CancellationToken cancellationToken);
        Task UpdateTruck(Truck truck, CancellationToken cancellationToken);
        Task DeleteTruck(Guid truckId, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
