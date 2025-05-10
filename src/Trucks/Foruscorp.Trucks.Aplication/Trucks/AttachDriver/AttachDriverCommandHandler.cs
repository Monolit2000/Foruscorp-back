using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Aplication.Contruct;

namespace Foruscorp.Trucks.Aplication.Trucks.AttachDriver
{
    public class AttachDriverCommandHandler(ITruckContext context) : IRequestHandler<AttachDriverCommand, Result>
    {
        public async Task<Result> Handle(AttachDriverCommand command, CancellationToken cancellationToken)
        {
            var truck = await context.Trucks.FirstOrDefaultAsync(t => t.Id == command.TruckId, cancellationToken);
            if (truck == null)
                return Result.Fail($"Truck with id {command.TruckId} not found.");

            var driver = await context.Drivers.FirstOrDefaultAsync(d => d.Id == command.DriverId, cancellationToken);
            if (driver == null)
                return Result.Fail($"Driver with id {command.DriverId} not found.");

            truck.AttachDriver(driver);

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
