using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Contruct
{
    public interface ITruckTrackingService
    {
        Task<int> GetNearFuelStationBonusAsync(Guid truckId, DateTime transactionTime, double Quantity, double tankCapacity = 200, CancellationToken cancellationToken = default);
    }
}
