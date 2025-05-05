using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Contruct.Samasara
{
    public class VehicleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Vin { get; set; }
        public string Serial { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string HarshAccelerationSettingType { get; set; }
        public string LicensePlate { get; set; }
        public int Year { get; set; }
        public DateTime CreatedAtTime { get; set; }
        public DateTime UpdatedAtTime { get; set; }
    }
}
