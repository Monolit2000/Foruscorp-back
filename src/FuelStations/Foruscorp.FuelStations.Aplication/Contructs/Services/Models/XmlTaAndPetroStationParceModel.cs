using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.Contructs.Services.Models
{
    public class XmlTaAndPetroStationParceModel
    {
        public string Id { get; set; }
        public double ECSCost { get; set; }
        public double Discount { get; set; }
        public string State { get; set; }
        public string TravelCenter { get; set; }
        public double Price { get; set; }
    }

    public class XmlTaAndPetroStationInfoModel
    {
        public string Id { get; set; }
        public string Brand { get; set; }
        public string Location { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Directions { get; set; }
        public string Address { get; set; }
    }
}
