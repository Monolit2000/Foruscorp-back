using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Drivers.SetUser
{
    public record SetUserCommand(Guid DriverId, Guid UserId) : IRequest;
    public class SetUserCommandHandler(
        ITruckContext truckContext,
        ILogger<SetUserCommandHandler> logger) : IRequestHandler<SetUserCommand>
    {
        public async Task Handle(SetUserCommand request, CancellationToken cancellationToken)
        {
            var driver = await truckContext.Drivers
                .FirstOrDefaultAsync(d => d.Id == request.DriverId);

            if (driver == null)
            {
                logger.LogWarning($"Driver with ID {request.DriverId} not found.");
                return;
            }

            driver.SetUser(request.UserId);   

            await truckContext.SaveChangesAsync(cancellationToken);
        }
    }
}
