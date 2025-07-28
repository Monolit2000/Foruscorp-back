using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRadius
{
    public class GetFuelStationsByRadiusQuery : IRequest<IEnumerable<FuelStation>> 
    {
        //decimal latitude { get; set; }
        //decimal Longitude { get; set; }
        //public int Radius { get; set; } 
        //public string source { get; set; }
        //public string destination { get; set; }
        //public string AddressPoint { get; set; }    


    }
}
