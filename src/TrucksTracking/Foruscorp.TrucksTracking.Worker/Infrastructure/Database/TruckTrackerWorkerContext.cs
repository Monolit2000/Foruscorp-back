using Foruscorp.TrucksTracking.Worker.Domain;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Worker.Infrastructure.Database
{
    public class TruckTrackerWorkerContext(DbContextOptions<TruckTrackerWorkerContext> options) : DbContext(options)
    {
        public DbSet<TruckTracker> TruckTrackers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("TruckTrackerWorker");
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TruckTrackerWorkerContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
