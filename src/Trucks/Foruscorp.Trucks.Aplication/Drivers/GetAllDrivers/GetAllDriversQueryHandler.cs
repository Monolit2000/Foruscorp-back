using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Aplication.Trucks;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.Trucks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Foruscorp.Trucks.Aplication.Drivers.GetAllDrivers
{
    public class GetAllDriversQueryHandler(ITruckContext context) : IRequestHandler<GetAllDriversQuery, List<GetAllDriverDto>>
    {
        public async Task<List<GetAllDriverDto>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
        {
            var drivers = await context.Drivers
                .AsNoTracking()
                .Include(d => d.Truck)
                .Include(d => d.Bonuses)
                .Include(d => d.DriverUser)
                    .ThenInclude(u => u.Contact)
                .ToListAsync(cancellationToken);

            if (!drivers.Any())
                return new List<GetAllDriverDto>();

            var driverDtos = drivers
                .Select(d => d.ToGetAlDriverDto())
                .ToList();

            return driverDtos;
        }
    }

    public class GetAllDriverDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int Bonus { get; set; }
        public string TelegramLink { get; set; }

        public GetAllTruckDto Truck { get; set; }
    }

    public class GetAllTruckDto
    {
        public Guid Id { get; set; }
        public string UnitNumber { get; set; }
        public string Vin { get; set; }
    }

    public static class DriverMapperExtencion
    {
        public static GetAllDriverDto ToGetAlDriverDto(this Driver driver)
        {
            return new GetAllDriverDto
            {
                Id = driver.Id,
                FullName = driver.DriverUser?.Contact?.FullName,
                Phone = driver.DriverUser?.Contact?.Phone,
                Email = driver.DriverUser?.Contact?.Email,
                Bonus = driver.TotalBonus,
                TelegramLink = driver.DriverUser?.Contact?.TelegramLink,
                Truck = driver.Truck == null
                    ? null
                    : new GetAllTruckDto
                    {
                        Id = driver.Truck.Id,
                        UnitNumber = driver.Truck.Name,
                        Vin = driver.Truck.Vin
                    }
            };
        }
    }

}
