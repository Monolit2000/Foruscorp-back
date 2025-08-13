using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.ModelTruckGroups.SetTruckGroupWeightFuelCapacity
{
    public class SetTruckGroupWeightFuelCapacityCommand : IRequest<Result>
    {
        public Guid TruckGroupId { get; set; }
        public double FuelCapacity { get; set; }
        public double Weight { get; set; }
    }

    public class SetTruckGroupWeightFuelCapacityCommandHandler(ITruckContext truckContext) : IRequestHandler<SetTruckGroupWeightFuelCapacityCommand, Result>
    {
        public async Task<Result> Handle(SetTruckGroupWeightFuelCapacityCommand request, CancellationToken cancellationToken)
        {
            var truckGroup = await truckContext.ModelTruckGroups
                .FirstOrDefaultAsync(m => m.Id == request.TruckGroupId, cancellationToken);

            if (truckGroup == null)
                return Result.Fail($"Truck group with ID {request.TruckGroupId} not found.");

            if (request.FuelCapacity <= 0)
                return Result.Fail("Fuel capacity must be greater than zero.");

            if (truckGroup == null)
                return Result.Fail($"Truck group with ID {request.TruckGroupId} not found.");

            if (request.Weight <= 0)
                return Result.Fail("Weight must be greater than zero.");

            truckGroup.SetFuelCapacity(request.FuelCapacity);
            truckGroup.SetAveregeWeight(request.Weight);
            await truckContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
