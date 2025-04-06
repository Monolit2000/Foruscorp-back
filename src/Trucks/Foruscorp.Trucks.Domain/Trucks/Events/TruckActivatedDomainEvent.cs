using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.Trucks.Domain.Trucks.Events
{
    public class TruckActivatedEvent : DomainEventBase
    {
        public Guid TruckId { get; }
        public string Ulid { get; }
        public DateTime ActivatedAt { get; }

        public TruckActivatedEvent(Guid truckId, string ulid)
        {
            TruckId = truckId;
            Ulid = ulid;
            ActivatedAt = DateTime.UtcNow;
        }
    }
}
