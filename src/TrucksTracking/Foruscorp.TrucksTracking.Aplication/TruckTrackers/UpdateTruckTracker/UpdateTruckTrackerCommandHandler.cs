using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker
{
    public class UpdateTruckTrackerCommandHandler(
        ITruckTrackingContext tuckTrackingContext,
        ILogger<UpdateTruckTrackerCommandHandler> logger) : IRequestHandler<UpdateTruckTrackerCommand>
    {
        public async Task Handle(UpdateTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            var truckId = Guid.Parse(request.TruckInfoStatsUpdate.TruckId);

            var truckTracker = await tuckTrackingContext.TruckTrackers
                .Include(tt => tt.CurrentTruckLocation)
                .Include(tt => tt.TruckLocationHistory)
                .FirstOrDefaultAsync(t => t.TruckId == truckId, cancellationToken);

            if (truckTracker == null)
            {
                logger.LogWarning("Truck Tracker not found for TruckId: {TruckId}", truckId);
                return;
            }

            truckTracker.UpdateTruckTracker(
                new GeoPoint(request.TruckInfoStatsUpdate.Latitude, request.TruckInfoStatsUpdate.Longitude),
                request.TruckInfoStatsUpdate.formattedLocation, 
                request.TruckInfoStatsUpdate.fuelPercents);

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);  
        }
    }
}
