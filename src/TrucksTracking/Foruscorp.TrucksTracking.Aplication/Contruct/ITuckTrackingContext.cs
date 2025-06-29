using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct
{
    public interface ITuckTrackingContext
    {
        DbSet<Route> Routes { get; set; }
        DbSet<TruckTracker> TruckTrackers { get; set; }
        DbSet<TruckLocation> TruckLocations { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
