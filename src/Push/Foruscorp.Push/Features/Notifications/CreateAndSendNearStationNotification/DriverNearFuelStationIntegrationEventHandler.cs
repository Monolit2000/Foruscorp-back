using Foruscorp.Trucks.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Foruscorp.Push.Features.Notifications.CreateAndSendNearStationNotification
{
    public class DriverNearFuelStationIntegrationEventHandler(
        ISender sender) : IConsumer<DriverNearFuelStationIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<DriverNearFuelStationIntegrationEvent> context)
        {
            await sender.Send(new CreateAndSendNearStationNotificationCommand(
                context.Message.UserId, 
                context.Message.FuelStationId, 
                context.Message.Address,
                context.Message.DistanceKm));
        }
    }
}
