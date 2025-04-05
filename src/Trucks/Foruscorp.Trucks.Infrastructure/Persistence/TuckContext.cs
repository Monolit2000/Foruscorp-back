using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Domain.Trucks;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.DriverFuelHistorys;

namespace Foruscorp.Trucks.Infrastructure.Persistence
{
    public class TuckContext : DbContext, ITuckContext
    {
        public DbSet<Truck> Trucks { get; set; }
        
        public DbSet<Driver> Drivers { get; set; }

        public DbSet<DriverBonus> DriverBonuses { get; set; }

        public DbSet<DriverFuelHistory> DriverFuelHistories { get; set; }


        public TuckContext(DbContextOptions<TuckContext> options) : base(options)
        {
        }
        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await base.SaveChangesAsync(cancellationToken);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("Tuck");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TuckContext).Assembly);
        }
    }
}
