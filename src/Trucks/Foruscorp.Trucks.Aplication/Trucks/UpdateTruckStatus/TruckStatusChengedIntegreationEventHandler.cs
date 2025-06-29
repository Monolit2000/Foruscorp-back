using Foruscorp.Trucks.IntegrationEvents;
using Foruscorp.TrucksTracking.IntegrationEvents;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Trucks.UpdateTruckStatus
{
    public class TruckStatusChengedIntegreationEventHandler(
        ISender sender) : IConsumer<TruckStatusChengedIntegreationEvent>
    {
        public async Task Consume(ConsumeContext<TruckStatusChengedIntegreationEvent> context)
        {
            await sender.Send(new UpdateTruckStatusCommand()
            {
                TruckId = context.Message.TruckId,
                Status = context.Message.StatusCode,
            });
        }
    }
}
