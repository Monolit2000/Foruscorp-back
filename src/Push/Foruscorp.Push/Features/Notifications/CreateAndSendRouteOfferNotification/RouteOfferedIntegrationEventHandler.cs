using Foruscorp.Trucks.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Foruscorp.Push.Features.Notifications.CreateAndSendRouteOfferNotification
{
    public class RouteOfferedIntegrationEventHandler(
        ISender sender) : IConsumer<RouteOfferedIntegrationEvent>
    {
        public Task Consume(ConsumeContext<RouteOfferedIntegrationEvent> context)
        {
            return sender.Send(new CreateAndSendRouteOfferNotificationCommand(
                context.Message.UserId,
                context.Message.RouteId), 
                context.CancellationToken);
        }
    }
}
