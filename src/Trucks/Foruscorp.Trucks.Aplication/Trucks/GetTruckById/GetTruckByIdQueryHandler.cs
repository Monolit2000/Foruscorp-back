using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Trucks.Aplication.Contruct;
using MassTransit;

namespace Foruscorp.Trucks.Aplication.Trucks.GetTruckById
{
    public class GetTruckByIdQueryHandler(
        ITruckContext truckContext) : IRequestHandler<GetTruckByIdQuery, Result<TruckDto>>
    {
        public async Task<Result<TruckDto>> Handle(GetTruckByIdQuery request, CancellationToken cancellationToken)
        {
            var truck = await truckContext.Trucks
                .Include(t => t.Driver)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == request.TruckId);

            if (truck == null)
                return Result.Fail("Truck not found");

            var truckDto = truck.ToTruckDto();

            return Result.Ok(truckDto); 

        }
    }
}
