using Foruscorp.FuelStations.Domain.FuelStations;
using Foruscorp.FuelStations.Domain.FuelMapProvaiders;

using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Domain.CompanyFuelPriceMultipliers;

namespace Foruscorp.FuelStations.Infrastructure.Percistence
{
    public class FuelStationContext : DbContext, IFuelStationContext
    {
        public DbSet<FuelStation> FuelStations { get; set; }
        public DbSet<FuelMapProvaider> FuelMapProvaiders { get; set; }
        public DbSet<CompanyFuelPriceMultiplier> CompanyPriceMultipliers { get; set; }
        

        public FuelStationContext(DbContextOptions<FuelStationContext> options) : base(options)
        {
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await base.SaveChangesAsync(cancellationToken);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("FuelStation");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuelStationContext).Assembly);
        }   

    }
}
