using Foruscorp.TrucksTracking.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckStateInfo
{
    public class UpdateTruckTrackerCommandHandler(
        ITuckTrackingContext tuckTrackingContext) : IRequestHandler<UpdateTruckTrackerCommand>
    {
        public async Task Handle(UpdateTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            var truck = await tuckTrackingContext.TruckTrackers
                .FirstOrDefaultAsync(t => t.Id == request.TruckId, cancellationToken);

            if (truck == null)
                return;
            
            truck.UpdateTruck(request.CurrentTruckLocation, request.FuelStatus);

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);  
        }
    }
}
