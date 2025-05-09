using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.Trucks.Aplication.Drivers;
using Foruscorp.Trucks.Domain.Drivers;

namespace Foruscorp.Trucks.Aplication.Trucks
{
    public class TruckDto
    {
        public Guid Id { get; set; }
        public string ProviderTruckId { get; set; }
        public string LicensePlate { get; set; }
        public string Status { get; set; }
        public Guid? DriverId { get; set; }
        public DriverDto Driver { get; set; }
        public string Name { get; set; }
        public string Vin { get; set; }
        public string Serial { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string HarshAccelerationSettingType { get; set; }
        public string Year { get; set; }
        public DateTime CreatedAtTime { get; set; }
        public DateTime UpdatedAtTime { get; set; }
    }
}
