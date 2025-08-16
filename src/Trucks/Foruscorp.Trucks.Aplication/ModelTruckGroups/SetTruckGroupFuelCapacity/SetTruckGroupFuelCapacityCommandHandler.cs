using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.ModelTruckGroups.SetTruckGroupFuelCapacity
{

    public class SetTruckGroupFuelCapacityCommand : IRequest<Result>
    {
        public Guid TruckGroupId { get; set; }
        public double FuelCapacity { get; set; }
    }

    public class SetTruckGroupFuelCapacityCommandHandler(ITruckContext truckContext) : IRequestHandler<SetTruckGroupFuelCapacityCommand, Result>
    {
        public async Task<Result> Handle(SetTruckGroupFuelCapacityCommand request, CancellationToken cancellationToken)
        {
            var truckGroup = await truckContext.ModelTruckGroups
                .FirstOrDefaultAsync(m => m.Id == request.TruckGroupId, cancellationToken);

            if (truckGroup == null)
                return Result.Fail($"Truck group with ID {request.TruckGroupId} not found.");

            if (request.FuelCapacity <= 0)
                return Result.Fail("Fuel capacity must be greater than zero.");

            truckGroup.SetFuelCapacity(request.FuelCapacity);
            await truckContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
