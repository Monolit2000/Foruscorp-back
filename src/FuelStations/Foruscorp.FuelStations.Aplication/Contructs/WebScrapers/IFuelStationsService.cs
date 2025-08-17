using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.Contructs.WebScrapers
{
    public interface IFuelStationsService
    {
        Task<List<FuelStationResponce>> GetFuelStations(
            string bearerToken,
            int radius = 15, 
            string source = "Lebanon, Kansas, США", 
            string destination = "Lebanon, Kansas, США");


        //Task LoversePilotParce();
    }
}
