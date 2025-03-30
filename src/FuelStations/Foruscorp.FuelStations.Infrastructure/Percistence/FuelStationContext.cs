using Foruscorp.FuelStations.Domain.FuelStations;
using Foruscorp.FuelStations.Domain.FuelMapProvaiders;

using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelStations.Aplication.Contructs;

namespace Foruscorp.FuelStations.Infrastructure.Percistence
{
    public class FuelStationContext : DbContext, IFuelStationContext
    {
        public DbSet<FuelStation> FuelStations { get; set; }
        public DbSet<FuelMapProvaider> FuelMapProvaiders { get; set; }
        

        public FuelStationContext(DbContextOptions<FuelStationContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("FuelStation");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuelStationContext).Assembly);
        }   

    }
}
