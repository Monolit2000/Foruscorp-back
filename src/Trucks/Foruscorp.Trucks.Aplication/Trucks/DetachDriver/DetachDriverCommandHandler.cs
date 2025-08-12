using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Aplication.Contruct;

namespace Foruscorp.Trucks.Aplication.Trucks.DetachDriver
{
    public class DetachDriverCommand : IRequest<Result>
    {
        public Guid TruckId { get; }
        public Guid DriverId { get; }

        public DetachDriverCommand(Guid truckId, Guid driverId)
        {
            TruckId = truckId;
            DriverId = driverId;
        }
    }

    public class DetachDriverCommandHandler(ITruckContext context) : IRequestHandler<DetachDriverCommand, Result>
    {
        public async Task<Result> Handle(DetachDriverCommand command, CancellationToken cancellationToken)
        {
            var truck = await context.Trucks
                .FirstOrDefaultAsync(t => t.Id == command.TruckId, cancellationToken);
            if (truck == null)
                return Result.Fail($"Truck with id {command.TruckId} not found.");

            var driver = await context.Drivers
                .FirstOrDefaultAsync(d => d.Id == command.DriverId, cancellationToken);
            if (driver == null)
                return Result.Fail($"Driver with id {command.DriverId} not found.");

            truck.DetachDriver();

            await context.SaveChangesAsync(cancellationToken);

            return Result.Ok();
        }
    }
}
