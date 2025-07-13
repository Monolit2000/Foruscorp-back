using Foruscorp.TrucksTracking.Worker.Domain;
using Foruscorp.TrucksTracking.Worker.Infrastructure.Database;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Worker.Features.TruckTrackerWorkers.AddTruckTracker
{

    public record AddTruckTrackerCommand(Guid TruckId, string ProviderTruckId) : IRequest;

    public class AddTruckTrackerCommandHandler(
        TruckTrackerWorkerContext truckTrackerWorkerContext) : IRequestHandler<AddTruckTrackerCommand>
    {
        public async Task Handle(AddTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            var IsTruckTracker = await truckTrackerWorkerContext.TruckTrackers
                .AnyAsync(t => t.TruckId == request.TruckId, cancellationToken);

            if (IsTruckTracker)
                return;

            var truckTracker = new TruckTracker(request.TruckId, request.ProviderTruckId);
            await truckTrackerWorkerContext.TruckTrackers.AddAsync(truckTracker, cancellationToken);
            await truckTrackerWorkerContext.SaveChangesAsync(cancellationToken);
        }
    }
}
