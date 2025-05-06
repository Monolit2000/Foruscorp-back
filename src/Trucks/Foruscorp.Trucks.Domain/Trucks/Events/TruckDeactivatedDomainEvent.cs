using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.Trucks.Domain.Trucks.Events
{
    public class TruckDeactivatedEvent : DomainEventBase
    {
        public Guid TruckId { get; }
        public string Ulid { get; }
        public DateTime DeactivatedAt { get; }

        public TruckDeactivatedEvent(
            Guid truckId)
        {
            TruckId = truckId;
            DeactivatedAt = DateTime.UtcNow;
        }
    }
}
