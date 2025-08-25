using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.Companys;
using Foruscorp.Trucks.Domain.DriverFuelHistorys;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.Reports;
using Foruscorp.Trucks.Domain.RouteOffers;
using Foruscorp.Trucks.Domain.Transactions;
using Foruscorp.Trucks.Domain.Trucks;
using Foruscorp.Trucks.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Infrastructure.Persistence
{
    public class TruckContext : DbContext, ITruckContext
    {
        public DbSet<Truck> Trucks { get; set; }
        public DbSet<ModelTruckGroup> ModelTruckGroups { get; set; }
        
        public DbSet<Driver> Drivers { get; set; }

        public DbSet<DriverBonus> DriverBonuses { get; set; }

        public DbSet<DriverFuelHistory> DriverFuelHistories { get; set; }

        public DbSet<User> Users { get; set; }   

        public DbSet<RouteOffer> RouteOffers { get; set; }

        public DbSet<Company> Companys { get; set; }
        public DbSet<CompanyManager> CompanyManagers { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ReportLoadAttempt> ReportLoadAttempts { get; set; }

        public DbSet<TruckUsage> TruckUsages { get; set; }
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
