using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.ModelTruckGroups.SetTruckGroupWeight
{

    public class SetTruckGroupWeightCommand : IRequest<Result>  
    {
        public Guid TruckGroupId { get; set; }
        public double Weight { get; set; }
    }

    public class SetTruckGroupWeightCommandHandler(ITruckContext truckContext) : IRequestHandler<SetTruckGroupWeightCommand, Result>
    {
        public async Task<Result> Handle(SetTruckGroupWeightCommand request, CancellationToken cancellationToken)
        {
            var truckGroup = await truckContext.ModelTruckGroups
                 .FirstOrDefaultAsync(m => m.Id == request.TruckGroupId, cancellationToken);
    
            if (truckGroup == null)
                return Result.Fail($"Truck group with ID {request.TruckGroupId} not found.");

            if (request.Weight <= 0)
                return Result.Fail("Weight must be greater than zero.");

            truckGroup.SetAveregeWeight(request.Weight);
            await truckContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
    }
}
