using Foruscorp.Trucks.IntegrationEvents;
using Foruscorp.TrucksTracking.IntegrationEvents;
using Foruscorp.Users.IntegrationEvents;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Users
{
    public class NewUserRegistratedIntegrationEventHandler(
        ISender sender) : IConsumer<NewUserRegistratedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<NewUserRegistratedIntegrationEvent> context)
        {
            await sender.Send(new CreateUserCommand {UserId = context.Message.UserId });
        }
    }
}
