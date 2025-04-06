using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Trucks.AttachDriver
{
    public class AttachDriverCommandHandler(ITuckContext context) : IRequestHandler<AttachDriverCommand, Result>
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
