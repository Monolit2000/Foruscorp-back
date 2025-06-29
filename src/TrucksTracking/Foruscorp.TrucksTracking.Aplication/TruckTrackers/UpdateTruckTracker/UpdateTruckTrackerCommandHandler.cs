using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker
{
    public class UpdateTruckTrackerCommandHandler(
        ITuckTrackingContext tuckTrackingContext,
        ILogger<UpdateTruckTrackerCommandHandler> logger) : IRequestHandler<UpdateTruckTrackerCommand>
    {
        public async Task Handle(UpdateTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            var truckId = Guid.Parse(request.TruckStatsUpdate.TruckId);

            var truckTracker = await tuckTrackingContext.TruckTrackers
                .Include(tt => tt.CurrentTruckLocation)
                .Include(tt => tt.TruckLocationHistory)
                .FirstOrDefaultAsync(t => t.TruckId == truckId, cancellationToken);

            if (truckTracker == null)
            {
                logger.LogWarning("Truck Tracker not found for TruckId: {TruckId}", truckId);
                return;
            }

            truckTracker.UpdateTruck(
                new GeoPoint(request.TruckStatsUpdate.Latitude, request.TruckStatsUpdate.Longitude),
                request.TruckStatsUpdate.formattedLocation, 
                request.TruckStatsUpdate.fuelPercents);

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);  
        }
    }
}
