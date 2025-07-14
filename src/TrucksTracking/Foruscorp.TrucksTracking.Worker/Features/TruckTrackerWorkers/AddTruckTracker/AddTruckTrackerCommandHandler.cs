using MediatR;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Worker.Domain;
using Foruscorp.TrucksTracking.Worker.Infrastructure.Database;

namespace Foruscorp.TrucksTracking.Worker.Features.TruckTrackerWorkers.AddTruckTracker
{
    public record AddTruckTrackerCommand(Guid TruckId, string ProviderTruckId) : IRequest;

    public class AddTruckTrackerCommandHandler(
        TruckTrackerWorkerContext truckTrackerWorkerContext) : IRequestHandler<AddTruckTrackerCommand>
    {
        public async Task Handle(AddTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            var truckTracker = await truckTrackerWorkerContext.TruckTrackers
                .FirstOrDefaultAsync(tt => tt.TruckId == request.TruckId || tt.ProviderTruckId == request.ProviderTruckId, cancellationToken);

            if (truckTracker != null)
            {
                await UpdateActiveTruckAsync(request, truckTracker, cancellationToken);
                return; 
            }

            var newTruckTracker = new TruckTracker(request.TruckId, request.ProviderTruckId);
            await truckTrackerWorkerContext.TruckTrackers.AddAsync(newTruckTracker, cancellationToken);
            await truckTrackerWorkerContext.SaveChangesAsync(cancellationToken);
        }

        private async Task UpdateActiveTruckAsync(AddTruckTrackerCommand request, TruckTracker truckTracker, CancellationToken cancellationToken)
        {
            truckTracker.UpdateTruckTracker(request.TruckId, request.ProviderTruckId);
            truckTrackerWorkerContext.TruckTrackers.Update(truckTracker);
            await truckTrackerWorkerContext.SaveChangesAsync(cancellationToken);
        }
    }
}
