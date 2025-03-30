using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using MediatR;
namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRadius
{
    public class GetFuelStationsByRadiusQuery : IRequest<IEnumerable<FuelStationDto>> 
    {
        decimal latitude { get; set; }
        decimal Longitude { get; set; }
        public int Radius { get; set; } 

        public string AddressPoint { get; set; }    

    }
}
