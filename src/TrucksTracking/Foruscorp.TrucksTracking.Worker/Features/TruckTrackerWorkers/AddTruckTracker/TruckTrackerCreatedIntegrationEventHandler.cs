using Foruscorp.TrucksTracking.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Foruscorp.TrucksTracking.Worker.Features.TruckTrackerWorkers.AddTruckTracker
{
    public class TruckTrackerCreatedIntegrationEventHandler(
        ISender sender) : IConsumer<TruckTrackerCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<TruckTrackerCreatedIntegrationEvent> context)
        {
            await sender.Send(new AddTruckTrackerCommand(context.Message.TruckId, context.Message.ProviderTruckId));
        }
    }
}
