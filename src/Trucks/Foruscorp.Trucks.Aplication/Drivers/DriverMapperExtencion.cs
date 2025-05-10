using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Aplication.Trucks;
using Foruscorp.Trucks.Domain.Drivers;
using Foruscorp.Trucks.Domain.Trucks;

namespace Foruscorp.Trucks.Aplication.Drivers
{
    public static class DriverMapperExtencion
    {
        public static DriverDto ToDriverDto(this Driver driver)
        {
            return new DriverDto
            {
                Id = driver.Id,
                TruckId = driver.TruckId ?? Guid.Empty,
                FullName = driver.FullName,
                Phone = driver.Contact?.Phone,
                Email = driver.Contact?.Email,
                Bonus = driver.TotalBonus,
                TelegramLink = driver.Contact?.TelegramLink
            };
        }
    }
}
