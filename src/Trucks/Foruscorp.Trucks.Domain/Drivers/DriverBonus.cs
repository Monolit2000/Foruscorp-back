using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.Trucks.Domain.Drivers
{
    public class DriverBonus : Entity
    {
        public Guid UserId { get; set; }    

        public Guid Id { get; private set; }
        public Guid DriverId { get; private set; }
        public int Amount { get; private set; }
        public DateTime AwardedAt { get; private set; }
        public string Reason { get; private set; }

        private DriverBonus() { }

        private DriverBonus(Guid driverId, int amount, string reason)
        {
            if (amount <= 0)
                throw new ArgumentException("Bonus amount must be positive.", nameof(amount));
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Reason cannot be empty.", nameof(reason));

            Id = Guid.NewGuid();
            DriverId = driverId;
            Amount = amount;
            AwardedAt = DateTime.UtcNow;
            Reason = reason;
        }

        public static DriverBonus CreateNew(
            Guid driverId,
            int amount,
            string reason)
        {
            return new DriverBonus(
                driverId, 
                amount,
                reason);
        }   


        internal void UpdateAmount(int newAmount)
        {
            if (newAmount <= 0)
                throw new ArgumentException("Bonus amount must be positive.", nameof(newAmount));

            Amount = newAmount;
        }
    }
}
