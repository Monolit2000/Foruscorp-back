using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Aplication.Contruct.Samasara;
using Foruscorp.Trucks.Domain.Trucks;

namespace Foruscorp.Trucks.Aplication.Trucks
{
    public static class TruckMapperExtencion
    {
        public static TruckDto ToTruckDto(this Truck truck)
        {
            return new TruckDto
            {
                Id = truck.Id,
                Ulid = truck.Ulid,
                LicensePlate = truck.LicensePlate,
                Status = truck.Status.ToString(),
                DriverId = truck.Driver == null ? Guid.Empty : truck.Driver.Id,
                Driver = truck.Driver == null ? null : new DriverDto(
                    truck.Driver.Id,
                    truck.Driver.FullName,
                    truck.Driver.Status.ToString()) 
            };
        }


        public static Truck ToTruck(this Vehicle vehicle)
        {
            return Truck.CreateNew(
                "ulid", 
                vehicle.Id, 
                vehicle.Vin,
                vehicle.Serial, 
                vehicle.Make, 
                vehicle.Model,
                vehicle.HarshAccelerationSettingType,
                vehicle.LicensePlate);
          
        }
    }
}
