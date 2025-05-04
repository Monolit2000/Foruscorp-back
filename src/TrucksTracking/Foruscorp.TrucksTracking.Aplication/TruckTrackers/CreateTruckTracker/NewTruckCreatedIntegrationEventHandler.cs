using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Foruscorp.Trucks.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.CreateTruckTracker
{
    public class NewTruckCreatedIntegrationEventHandler(
        ISender sender) : IConsumer<TruckCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<TruckCreatedIntegrationEvent> context)
        {
            await sender.Send(new CreateTruckTrackerCommand() 
            {
                TruckId = context.Message.TruckId,
                ProviderTruckId = context.Message.ProviderTruckId,      
            });
        }
    }
}
