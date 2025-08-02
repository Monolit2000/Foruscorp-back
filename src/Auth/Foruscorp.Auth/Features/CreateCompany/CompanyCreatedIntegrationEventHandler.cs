using Foruscorp.Trucks.IntegrationEvents;
using MassTransit;
using MediatR;

namespace Foruscorp.Auth.Features.CreateCompany
{
    public class CompanyCreatedIntegrationEventHandler(ISender sender) : IConsumer<CompanyCreatedIntegrationEvent>
    {
        public async Task Consume(ConsumeContext<CompanyCreatedIntegrationEvent> context)
        {
            await sender.Send(new CreateCompanyCommand(context.Message.CompanyId, context.Message.Name));
        }
    }
}
