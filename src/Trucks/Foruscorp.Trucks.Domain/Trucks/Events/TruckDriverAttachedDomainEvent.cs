using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.Trucks.Domain.Trucks.Events
{
    public class TruckDriverAttachedDomainEvent : DomainEventBase
    {
        public Guid TruckId { get; }
        public Guid DriverId { get; }
        public DateTime AttachedAt { get; }
        public TruckDriverAttachedDomainEvent(
            Guid truckId,
            Guid driverId)
        {
            TruckId = truckId;
            DriverId = driverId;
            AttachedAt = DateTime.UtcNow;
        }
    }
}
