using MediatR;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.Trucks;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Drivers.GetAllDrivers
{
    public class GetAllDriversQueryHandler(ITuckContext context) : IRequestHandler<GetAllDriversQuery, List<DriverDto>>
    {

        public async Task<List<DriverDto>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
        {
            var drivers = await context.Drivers
                .AsNoTracking()
                .Include(d => d.Truck)
                .ToListAsync();

            if (!drivers.Any())
                return new List<DriverDto>();

            var driverDtos = drivers.Select(d => new DriverDto
            {
                Id = d.Id,
                FullName = d.FullName,
                TruckId = d.TruckId ?? Guid.Empty 

            }).ToList();

            return driverDtos;
        }

    }
}
