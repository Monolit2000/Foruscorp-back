using System;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Aplication.Contruct
{
    public interface ITuckTrackingContext
    {
        DbSet<TruckTracker> TruckTrackers { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
