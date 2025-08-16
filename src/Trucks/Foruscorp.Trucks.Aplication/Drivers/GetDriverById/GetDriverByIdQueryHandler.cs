using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Drivers.GetDriverById
{
    public record GetDriverByIdQuery(Guid DriverId) : IRequest<Result<DriverDto>>;
    public class GetDriverByIdQueryHandler (ITruckContext context) : IRequestHandler<GetDriverByIdQuery, Result<DriverDto>>
    {
        public async Task<Result<DriverDto>> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
        {
            var driver = await context.Drivers
                .AsNoTracking()
                .Include(d => d.Bonuses)
                .Include(d => d.Truck)
                .Include(d => d.DriverUser)
                    .ThenInclude(u => u.Contact)
                .FirstOrDefaultAsync(d => d.Id == request.DriverId, cancellationToken);
            if (driver == null)
                return Result.Fail($"Driver with ID {request.DriverId} not found.");

            return driver.ToDriverDto();
        }
    }
}
