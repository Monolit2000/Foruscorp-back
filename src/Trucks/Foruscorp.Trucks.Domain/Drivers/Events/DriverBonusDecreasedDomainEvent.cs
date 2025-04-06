using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.Trucks.Domain.Drivers.Events
{
    public class DriverBonusDecreasedDomainEvent : DomainEventBase
    {
        public Guid DriverId { get; }
        public decimal Amount { get; }
        public string Reason { get; }
        public DriverBonusDecreasedDomainEvent(Guid driverId, decimal amount, string reason)
        {
            DriverId = driverId;
            Amount = amount;
            Reason = reason;
        }
    }
}
