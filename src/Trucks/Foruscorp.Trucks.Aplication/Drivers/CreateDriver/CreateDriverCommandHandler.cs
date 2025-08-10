using MediatR;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Aplication.Contruct;
using MassTransit.Testing;
using Microsoft.EntityFrameworkCore;
using FluentResults;

namespace Foruscorp.Trucks.Aplication.Drivers.CreateDriver
{
    public class CreateDriverCommand : IRequest<Result<DriverDto>>
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TelegramLink { get; set; }
    }

    public class CreateDriverCommandHandler(ITruckContext context) : IRequestHandler<CreateDriverCommand, Result<DriverDto>>
    {
        public async Task<Result<DriverDto>> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            var user = await context.Users
                .Include(u => u.Contact)
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            if(user == null)
                return Result.Fail("User not found.");

            var driver = Driver.CreateNew(user.UserId ,request.Name, request.Phone, request.Email, request.TelegramLink);

            await context.Drivers.AddAsync(driver, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            return driver.ToDriverDto();
        }
    }   
}
