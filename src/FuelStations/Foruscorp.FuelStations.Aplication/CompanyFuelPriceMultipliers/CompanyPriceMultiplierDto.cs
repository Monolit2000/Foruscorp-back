using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.CompanyPriceMultipliers
{
    public class CompanyPriceMultiplierDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public double PriceMultiplier { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ChangedAt { get; set; }
        public CompanyPriceMultiplierDto(
            Guid id,
            Guid companyId,
            double priceMultiplier,
            DateTime createdAt,
            DateTime changedAt)
        {
            Id = id;
            CompanyId = companyId;
            PriceMultiplier = priceMultiplier;
            CreatedAt = createdAt;
            ChangedAt = changedAt;
        }
    }
}
