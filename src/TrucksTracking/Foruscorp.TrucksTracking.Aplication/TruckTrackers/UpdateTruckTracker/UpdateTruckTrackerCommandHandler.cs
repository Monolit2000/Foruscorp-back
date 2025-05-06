using Foruscorp.TrucksTracking.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker
{
    public class UpdateTruckTrackerCommandHandler(
        ITuckTrackingContext tuckTrackingContext) : IRequestHandler<UpdateTruckTrackerCommand>
    {
        public async Task Handle(UpdateTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            var truckTracker = await tuckTrackingContext.TruckTrackers
                .FirstOrDefaultAsync(t => t.Id == request.TruckId, cancellationToken);

            if (truckTracker == null)
                return;

            truckTracker.UpdateTruck(request.CurrentTruckLocation, request.FuelStatus);

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);  
        }
    }
}
