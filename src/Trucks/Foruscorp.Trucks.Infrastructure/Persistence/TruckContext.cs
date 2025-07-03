using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Domain.Trucks;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.DriverFuelHistorys;
using Foruscorp.Trucks.Domain.Users;

namespace Foruscorp.Trucks.Infrastructure.Persistence
{
    public class TruckContext : DbContext, ITruckContext
    {
        public DbSet<Truck> Trucks { get; set; }
        
        public DbSet<Driver> Drivers { get; set; }

        public DbSet<DriverBonus> DriverBonuses { get; set; }

        public DbSet<DriverFuelHistory> DriverFuelHistories { get; set; }

        public DbSet<User> Users { get; set; }   


        public TruckContext(DbContextOptions<TruckContext> options) : base(options)
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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TruckContext).Assembly);
        }
    }
}
