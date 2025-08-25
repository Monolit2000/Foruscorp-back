using Foruscorp.Trucks.Domain.Drivers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Domain.Trucks
{
    public class TruckUsage
    {
        public Guid Id { get; set; }

        public Guid TruckId { get; set; }
        public Truck Truck { get; set; }

        public Guid DriverId { get; set; }
        public Driver Driver { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }

        private TruckUsage(Guid truckId, Guid driverId)
        {
            Id = Guid.NewGuid();
            TruckId = truckId;  
            DriverId = driverId;
            StartedAt = DateTime.UtcNow;
        }

        public static TruckUsage CreateNew(Guid truckId, Guid driverId)
        {
            return new TruckUsage(truckId, driverId);
        }   

        public void EndUsage()
        {
            EndedAt = DateTime.UtcNow;
        }
    }
}
