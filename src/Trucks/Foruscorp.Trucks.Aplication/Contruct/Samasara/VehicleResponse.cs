using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Contruct.Samasara
{
    // Data models
    public class VehicleResponse
    {
        public Vehicle[] Data { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class Vehicle
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Vin { get; set; }
        public string Serial { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Year { get; set; }
        public string LicensePlate { get; set; }
        public string Notes { get; set; }
        public string HarshAccelerationSettingType { get; set; }
        public DateTime CreatedAtTime { get; set; }
        public DateTime UpdatedAtTime { get; set; }
        public ExternalIds ExternalIds { get; set; }
    }

    public class ExternalIds
    {
        public string SamsaraSerial { get; set; }
        public string SamsaraVin { get; set; }
    }

    public class Pagination
    {
        public string EndCursor { get; set; }
        public bool HasNextPage { get; set; }
    }
}
