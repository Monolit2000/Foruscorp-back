using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelStations.Domain.FuelStations;
using Foruscorp.FuelStations.Domain.FuelMapProvaiders;
using Foruscorp.FuelStations.Domain.CompanyFuelPriceMultipliers;

namespace Foruscorp.FuelStations.Aplication.Contructs
{
    public interface IFuelStationContext
    {
        public DbSet<FuelStation> FuelStations { get; set; }
        public DbSet<FuelMapProvaider> FuelMapProvaiders { get; set; }
        public DbSet<CompanyFuelPriceMultiplier> CompanyPriceMultipliers { get; set; }

        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
