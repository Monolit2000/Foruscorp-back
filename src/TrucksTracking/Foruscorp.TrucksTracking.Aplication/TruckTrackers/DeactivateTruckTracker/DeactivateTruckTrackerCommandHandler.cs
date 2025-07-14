using MediatR;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.CreateTruckTracker;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.DeactivateTruckTracker
{
    public class DeactivateTruckTrackerCommandHandler(
        ActiveTruckManager activeTruckManager,
        ITruckTrackingContext tuckTrackingContext,
        ILogger<CreateTruckTrackerCommandHandler> logger) : IRequestHandler<DeactivateTruckTrackerCommand, Result>
    {
        public async Task<Result> Handle(DeactivateTruckTrackerCommand request, CancellationToken cancellationToken)
        {
           var truckTracker = await tuckTrackingContext.TruckTrackers
                .FirstOrDefaultAsync(tt => tt.TruckId == request.TruckId, cancellationToken);

            if (truckTracker == null)
                return Result.Fail("TruckTracker not exist");

            truckTracker.DeactivateTruckTrucker();
            await tuckTrackingContext.SaveChangesAsync(cancellationToken);

            activeTruckManager.RemoveTruck(truckTracker.TruckId.ToString());
            logger.LogInformation("TruckTracker deactivated with TruckId: {TruckId}", request.TruckId);

            return Result.Ok(); 
        }
    }   
}
