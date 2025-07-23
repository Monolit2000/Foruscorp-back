using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.FuelStations.PlanNearFuelStation
{
    public class FuelStationPlanedIntegrationEventHandler(
        ISender sender) : IConsumer<FuelStationPlanAssignedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<FuelStationPlanAssignedIntegrationEvent> context)
        {
            await sender.Send(new PlanNearFuelStationCommand(
                context.Message.FuelStationId, context.Message.RouteId, context.Message.TruckId, context.Message.Address, context.Message.NearDistance, context.Message.Longitude, context.Message.Latitude));
        }
    }
}
