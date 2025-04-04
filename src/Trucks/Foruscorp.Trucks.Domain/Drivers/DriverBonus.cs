using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Domain.Drivers
{
    public class DriverBonus
    {
        public Guid Id { get; private set; }
        public Guid DriverId { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime AwardedAt { get; private set; }
        public string Reason { get; private set; }

        private DriverBonus() { }

        internal DriverBonus(Guid driverId, decimal amount, string reason)
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

        internal void UpdateAmount(decimal newAmount)
        {
            if (newAmount <= 0)
                throw new ArgumentException("Bonus amount must be positive.", nameof(newAmount));

            Amount = newAmount;
        }
    }
}
