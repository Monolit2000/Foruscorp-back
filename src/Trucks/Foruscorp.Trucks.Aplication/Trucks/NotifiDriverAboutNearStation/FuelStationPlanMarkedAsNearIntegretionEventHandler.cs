using Foruscorp.TrucksTracking.IntegrationEvents;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Trucks.NotifiDriverAboutNearStation
{
    public class FuelStationPlanMarkedAsNearIntegretionEventHandler(ISender sender) : IConsumer<FuelStationPlanMarkedAsNearIntegretionEvent>
    {
        public async Task Consume(ConsumeContext<FuelStationPlanMarkedAsNearIntegretionEvent> context)
        {
            await sender.Send(new NotifyDriverAboutNearStationCommand(
                    context.Message.TruckId, 
                    context.Message.FuelStationId, 
                    context.Message.Address,
                    context.Message.Longitude,
                    context.Message.Latitude, 
                    context.Message.DistanceKm));
        }
    }
}
