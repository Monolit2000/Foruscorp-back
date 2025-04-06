using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Foruscorp.Trucks.IntegrationEvents;
using MassTransit;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.CreateTruckTracker
{
    public class NewTruckCreatedIntegrationEventHandler : IConsumer<TruckCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<TruckCreatedIntegrationEvent> context)
        {
            Console.WriteLine($"{context.Message.TruckId}");
        }
    }
}
