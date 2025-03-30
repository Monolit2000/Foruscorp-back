using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.FuelStations
{
    public class FuelStationDto
    {
        public int Id { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Price { get; set; }
        public string Discount { get; set; }
        public string PriceAfterDiscount { get; set; }
        public string DistanceToLocation { get; set; }
        public int Route { get; set; }
    }
}
