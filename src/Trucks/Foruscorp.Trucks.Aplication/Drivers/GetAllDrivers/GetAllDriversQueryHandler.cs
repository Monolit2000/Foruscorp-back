using MediatR;
using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.Trucks;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Drivers.GetAllDrivers
{
    public class GetAllDriversQueryHandler(ITruckContext context) : IRequestHandler<GetAllDriversQuery, List<DriverDto>>
    {
        public async Task<List<DriverDto>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
        {
            var drivers = await context.Drivers
                .AsNoTracking()
                .Include(d => d.Contact)
                .Include(d => d.Truck)
                .ToListAsync();

            if (!drivers.Any())
                return new List<DriverDto>();

            var driverDtos = drivers
                .Select(d => d.ToDriverDto())
                .ToList();

            return driverDtos;
        }
    }
}
