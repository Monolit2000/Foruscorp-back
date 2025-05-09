using MediatR;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Aplication.Contruct;

namespace Foruscorp.Trucks.Aplication.Drivers.CreateDriver
{
    public class CreateDriverCommandHandler(ITruckContext context) : IRequestHandler<CreateDriverCommand, DriverDto>
    {
        public async Task<DriverDto> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            var driver = Driver.CreateNew(request.Name, request.Phone, request.Email, request.TelegramLink);

            await context.Drivers.AddAsync(driver, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return driver.ToDriverDto();
        }
    }   
}
