using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.Aplication.Contruct;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.CreateTruckTracker
{
    public class CreateTruckTrackerCommandHandler(
        ITuckTrackingContext tuckTrackingContext,
        ILogger<CreateTruckTrackerCommandHandler> logger) : IRequestHandler<CreateTruckTrackerCommand>
    {
        public async Task Handle(CreateTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            if (await tuckTrackingContext.TruckTrackers.AnyAsync(tt => tt.TruckId == request.TruckId))
                return;

            var truckTracker = TruckTracker.Create(request.TruckId);

            await tuckTrackingContext.TruckTrackers.AddAsync(truckTracker, cancellationToken);  

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("TruckTracker created with TruckId: {TruckId}", request.TruckId); 
        }
    }
}
