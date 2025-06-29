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
            var truckTracker = await tuckTrackingContext.TruckTrackers
                .Include(tt => tt.CurrentTruckLocation)
                .Include(tt => tt.TruckLocationHistory)
                .FirstOrDefaultAsync(t => t.TruckId == request.TruckId, cancellationToken);

            if (truckTracker == null)
            {
                logger.LogWarning("Truck Tracker not found for TruckId: {TruckId}", request.TruckId);
                return;
            }

            truckTracker.UpdateTruck(
                new GeoPoint(request.truckStatsUpdate.Latitude, request.truckStatsUpdate.Longitude),
                request.truckStatsUpdate.formattedLocation, 
                request.truckStatsUpdate.fuelPercents);

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);  
        }
    }
}
