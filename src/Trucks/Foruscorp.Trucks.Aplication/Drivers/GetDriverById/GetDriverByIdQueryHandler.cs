using FluentResults;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Aplication.Drivers.GetAllDrivers;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Drivers.GetDriverById
{
    public record GetDriverByIdQuery(Guid DriverId) : IRequest<Result<GetAllDriverDto>>;
    public class GetDriverByIdQueryHandler (ITruckContext context) : IRequestHandler<GetDriverByIdQuery, Result<GetAllDriverDto>>
    {
        public async Task<Result<GetAllDriverDto>> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
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

            return driver.ToGetAlDriverDto();
        }
    }
}
