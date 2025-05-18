using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.FuelStations.Domain.CompanyFuelPriceMultipliers
{
    public class CompanyFuelPriceMultiplier : Entity, IAggregateRoot
    {
        public Guid CompanyId { get; private set; }
        public Guid FuelProviderId { get; private set; }    
        public Guid Id { get; private set; }
        public double PriceMultiplier { get; private set; } // 1.0 = 100% of the price, 0.9 = 90% of the price
        public DateTime CreatedAt { get; private set; }
        public DateTime ChangedAt { get; private set; }
        private CompanyFuelPriceMultiplier() { } // For EF core
        private CompanyFuelPriceMultiplier(Guid companyId, double priceMultiplier)
        {
            if (priceMultiplier <= 0)
                throw new ArgumentException("Price multiplier must be greater than zero", nameof(priceMultiplier));
            Id = Guid.NewGuid();
            CompanyId = companyId;
            PriceMultiplier = priceMultiplier;
            CreatedAt = DateTime.UtcNow;
            ChangedAt = DateTime.UtcNow;
        }
        public static CompanyFuelPriceMultiplier CreateNew(Guid companyId, double priceMultiplier)
        {
            return new CompanyFuelPriceMultiplier(companyId, priceMultiplier);
        }
        public void UpdatePriceMultiplier(double newPriceMultiplier)
        {
            if (newPriceMultiplier <= 0)
                throw new ArgumentException("Price multiplier must be greater than zero", nameof(newPriceMultiplier));
            PriceMultiplier = newPriceMultiplier;
            ChangedAt = DateTime.UtcNow;
        }
    }
}
