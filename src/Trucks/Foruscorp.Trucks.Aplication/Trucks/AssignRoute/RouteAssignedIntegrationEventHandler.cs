using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Foruscorp.Trucks.Aplication.Trucks.AssignRoute
{
    public class RouteAssignedIntegrationEventHandler(
        ISender sender) : IConsumer<RouteAssignedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<RouteAssignedIntegrationEvent> context)
        {
            await sender.Send(new AssignRouteCommand(
                context.Message.RouteId, 
                context.Message.TruckId,
                context.Message.IsSelfAssign), 
                context.CancellationToken);
        }
    }
}
