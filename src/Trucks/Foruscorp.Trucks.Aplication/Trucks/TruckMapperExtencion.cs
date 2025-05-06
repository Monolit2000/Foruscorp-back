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
                ProviderTruckId = truck.ProviderTruckId,
                LicensePlate = truck.LicensePlate,
                Status = truck.Status.ToString(),
                DriverId = truck.Driver == null ? Guid.Empty : truck.Driver.Id,
                Driver = truck.Driver == null ? null : new DriverDto(
                    truck.Driver.Id,
                    truck.Driver.FullName,
                    truck.Driver.Status.ToString()),
                Name = truck.Name,
                Vin = truck.Vin,
                Serial = truck.Serial,
                Make = truck.Make,
                Model = truck.Model,
                HarshAccelerationSettingType = truck.HarshAccelerationSettingType,
                Year = truck.Year,
                CreatedAtTime = truck.CreatedAtTime,
                UpdatedAtTime = truck.UpdatedAtTime
            };
        }


        public static Truck ToTruck(this Vehicle vehicle)
        {
            return Truck.CreateNew(
                "ulid",
                vehicle.Name,
                vehicle.Id, 
                vehicle.Vin,
                vehicle.Serial, 
                vehicle.Make, 
                vehicle.Model,
                vehicle.HarshAccelerationSettingType,
                vehicle.LicensePlate,
                vehicle.Year,
                vehicle.CreatedAtTime,
                vehicle.UpdatedAtTime);
        }

        public static TruckDto ToTruckDto(this Vehicle vehicle)
        {
            return new TruckDto
            {
                ProviderTruckId = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Status = string.Empty, // Vehicle may not have Status; set default or adjust as needed
                DriverId = Guid.Empty, // Vehicle may not have Driver; set default
                Driver = null,         // Vehicle may not have Driver; set default
                Name = vehicle.Name,
                Vin = vehicle.Vin,
                Serial = vehicle.Serial,
                Make = vehicle.Make,
                Model = vehicle.Model,
                HarshAccelerationSettingType = vehicle.HarshAccelerationSettingType,
                Year = vehicle.Year,
                CreatedAtTime = vehicle.CreatedAtTime,
                UpdatedAtTime = vehicle.UpdatedAtTime
            };
        }
    }
}
