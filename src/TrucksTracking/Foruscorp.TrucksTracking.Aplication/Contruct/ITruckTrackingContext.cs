using Foruscorp.TrucksTracking.Domain.FuelStationPlans;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.Domain.Transactions;
using Foruscorp.TrucksTracking.Domain.Reports;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Aplication.Contruct
{
    public interface ITruckTrackingContext
    {
        DbSet<Route> Routes { get; set; }
        DbSet<TruckTracker> TruckTrackers { get; set; }
        DbSet<TruckLocation> TruckLocations { get; set; }
        DbSet<NearFuelStationPlan> NearFuelStationPlans { get; set; }
        DbSet<Transaction> Transactions { get; set; }
        DbSet<ReportLoadAttempt> ReportLoadAttempts { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
