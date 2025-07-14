using Foruscorp.Trucks.IntegrationEvents;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.IntegrationEvents;
using MassTransit;
using MassTransit.Transports;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.CreateTruckTracker
{
    public class CreateTruckTrackerCommandHandler(
        IPublishEndpoint publishEndpoint,
        ActiveTruckManager activeTruckManager,
        ITuckTrackingContext tuckTrackingContext,
        ILogger<CreateTruckTrackerCommandHandler> logger) : IRequestHandler<CreateTruckTrackerCommand>
    {
        public async Task Handle(CreateTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            var truckTracker = await tuckTrackingContext.TruckTrackers
                .FirstOrDefaultAsync(tt => tt.TruckId == request.TruckId || tt.ProviderTruckId == request.ProviderTruckId);
            if (truckTracker != null)
            {
                await UpdateActiveTruckAsync(request, truckTracker, cancellationToken);
                await PublishToMassageBrocker(truckTracker);
                return;
            }

            var NewTruckTracker = TruckTracker.Create(request.TruckId, request.ProviderTruckId);
            await tuckTrackingContext.TruckTrackers.AddAsync(NewTruckTracker, cancellationToken);
            await tuckTrackingContext.SaveChangesAsync(cancellationToken);

            await PublishToMassageBrocker(NewTruckTracker); 

            logger.LogInformation("TruckTracker created with TruckId: {TruckId}", request.TruckId);
        }

        private async Task UpdateActiveTruckAsync(CreateTruckTrackerCommand request, TruckTracker truckTracker, CancellationToken cancellationToken)
        {
            truckTracker.UpdateTruckTracker(request.TruckId, request.ProviderTruckId);
            tuckTrackingContext.TruckTrackers.Update(truckTracker);
            await tuckTrackingContext.SaveChangesAsync(cancellationToken);
            return;
        }

        private async Task PublishToMassageBrocker(TruckTracker truckTracker) 
            => await publishEndpoint.Publish(new TruckTrackerCreatedIntegrationEvent(truckTracker.TruckId, truckTracker.Id, truckTracker.ProviderTruckId));
    }
}
