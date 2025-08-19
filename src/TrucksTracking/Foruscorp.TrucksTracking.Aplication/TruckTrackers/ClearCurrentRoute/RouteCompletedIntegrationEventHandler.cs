using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.ClearCurrentRoute
{
    public class RouteCompletedIntegrationEventHandler(
        ISender sender,
        ILogger<RouteCompletedIntegrationEventHandler> logger) : IConsumer<RouteCompletedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<RouteCompletedIntegrationEvent> context)
        {
            var message = context.Message;
            
            logger.LogInformation(
                "Route {RouteId} completed for truck {TruckId} at {CompletedAt}. Clearing current route.", 
                message.RouteId, 
                message.TruckId, 
                message.CompletedAt);

            await sender.Send(new ClearCurrentRouteCommand
            {
                TruckId = message.TruckId
            }, context.CancellationToken);
        }
    }
}
