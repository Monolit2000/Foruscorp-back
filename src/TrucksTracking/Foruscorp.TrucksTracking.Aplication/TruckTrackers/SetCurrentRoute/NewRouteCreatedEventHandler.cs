using Foruscorp.FuelRoutes.IntegrationEvents;
using Foruscorp.Trucks.IntegrationEvents;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.CreateTruckTracker;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.SetCurrentRoute
{
    public class NewRouteCreatedEventHandler(
          ISender sender) : IConsumer<RouteCreatedIntegretionEvent>
    {
        public async Task Consume(ConsumeContext<RouteCreatedIntegretionEvent> context)
        {
            await sender.Send(new SetCurrentRouteCommand()
            {
                TruckId = context.Message.TruckId, 
                RouteId = context.Message.RouteId,
            });
        }
    }
}
