using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Trucks.GetTruckByUserId
{
    public record GetTruckByUserIdQuery(Guid UserId) : IRequest<Result<TruckDto>>;
    public class GetTruckByUserIdQueryHandler(ITruckContext truckContext) : IRequestHandler<GetTruckByUserIdQuery, Result<TruckDto>>
    {
        public async Task<Result<TruckDto>> Handle(GetTruckByUserIdQuery request, CancellationToken cancellationToken)
        {
            var driver = await truckContext.Drivers
                .Include(d => d.Truck)
                .Include(d => d.DriverUser)
                    .ThenInclude(u => u.Contact)
                .FirstOrDefaultAsync(d => d.UserId == request.UserId, cancellationToken);

            if (driver == null)
            {
                return Result.Fail(new Error("Driver not found for the given user."));
            }

            var truck = driver.Truck;

            if (truck == null)
            {
                return Result.Fail(new Error("Truck not found for the given user."));
            }

            var truckDto = truck.ToTruckDto();

            return Result.Ok(truckDto); 
        }
    }
}
