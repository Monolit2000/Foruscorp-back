using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.Aplication.Contruct;

namespace Foruscorp.TrucksTracking.Infrastructure.Percistence
{
    public class TuckTrackingContext : DbContext, ITuckTrackingContext
    {

        public DbSet<TruckTracker> Trucks { get; set; }

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
