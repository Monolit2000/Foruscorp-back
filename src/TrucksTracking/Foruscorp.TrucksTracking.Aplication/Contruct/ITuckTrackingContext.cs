using Foruscorp.TrucksTracking.Domain.FuelStationPlans;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Aplication.Contruct
{
    public interface ITuckTrackingContext
    {
        DbSet<Route> Routes { get; set; }
        DbSet<TruckTracker> TruckTrackers { get; set; }
        DbSet<TruckLocation> TruckLocations { get; set; }
        DbSet<NearFuelStationPlan> NearFuelStationPlans { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
