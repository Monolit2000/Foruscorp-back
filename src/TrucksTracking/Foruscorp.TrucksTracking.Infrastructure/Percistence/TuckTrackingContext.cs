using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Domain.FuelStationPlans;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Infrastructure.Percistence
{
    public class TuckTrackingContext : DbContext, ITruckTrackingContext
    {

        public DbSet<TruckTracker> TruckTrackers { get; set; }
        public DbSet<TruckLocation> TruckLocations { get; set; }
        public DbSet<TruckFuel> TruckFuels { get; set; }
        public DbSet<Route> Routes { get; set; }    
        public DbSet<NearFuelStationPlan> NearFuelStationPlans { get; set; }
        public DbSet<TransactionReport> TransactionReports { get; set; }
        public DbSet<TransactionFill> TransactionFills { get; set; }
        public DbSet<TransactionItem> TransactionItems { get; set; }    



        public TuckTrackingContext(DbContextOptions<TuckTrackingContext> options) : base(options)   
        {
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await base.SaveChangesAsync(cancellationToken);    
        }   

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("TuckTracking");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TuckTrackingContext).Assembly);
        }
    }
}
