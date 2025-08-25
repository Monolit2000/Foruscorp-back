using MediatR;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.IntegrationEvents;
using MassTransit;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using FluentResults;

namespace Foruscorp.Trucks.Aplication.Drivers.CreateDriver
{
    public class CreateDriverCommand : IRequest<Result<DriverDto>>
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TelegramLink { get; set; }
    }

    public class CreateDriverCommandHandler(
        ITruckContext context,
        ICurrentUser currentUser,
        IPublishEndpoint publishEndpoint) : IRequestHandler<CreateDriverCommand, Result<DriverDto>>
    {
        public async Task<Result<DriverDto>> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            var driver = Driver.CreateNew(request.Name, request.Phone, request.Email, request.TelegramLink);

            driver.SetCompany(currentUser.CompanyId.Value);

            await context.Drivers.AddAsync(driver, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            await publishEndpoint.Publish(new DriverCreatedIntegrationEvent(
                driver.Id, 
                driver.UserId.Value, 
                request.Name,
                request.Phone,
                request.Email,
                request.TelegramLink), 
                cancellationToken);

            return driver.ToDriverDto();
        }
    }   
}
