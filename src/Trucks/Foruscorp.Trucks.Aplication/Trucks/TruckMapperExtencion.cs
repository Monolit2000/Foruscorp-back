using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Domain.Trucks;

namespace Foruscorp.Trucks.Aplication.Trucks
{
    public static class TruckMapperExtencion
    {

        public static TruckDto TrucToTruckDto(this Truck truck)
        {
            return new TruckDto
            {
                Id = truck.Id,
                Ulid = truck.Ulid,
                LicensePlate = truck.LicensePlate,
                Status = truck.Status.ToString(),
                DriverId = truck.DriverId
            };
        }

        public static TruckDto ToTruckDto(this Truck truck)
        {
            return new TruckDto
            {
                Id = truck.Id,
                Ulid = truck.Ulid,
                LicensePlate = truck.LicensePlate,
                Status = truck.Status.ToString(),
                DriverId = truck.DriverId
            };
        }

    }
}
