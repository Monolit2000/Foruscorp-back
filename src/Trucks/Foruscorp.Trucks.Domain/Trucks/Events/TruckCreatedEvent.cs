using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.Trucks.Domain.Trucks.Events
{
    public class TruckCreatedEvent : DomainEventBase
    {
        public TruckCreatedEvent(Truck truck)
        {
            Truck = truck;
        }
        public Truck Truck { get; private set; }
    }
}
