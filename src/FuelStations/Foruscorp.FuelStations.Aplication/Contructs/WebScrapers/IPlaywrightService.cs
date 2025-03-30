using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.Contructs.WebScrapers
{
    public interface IPlaywrightService
    {
        //Task<FuelPricesResponse> GetFuelPrices();

        Task<T> ParseDataByUri<T>();

        Task<object> ParseDataByUrl(object payload, string url);

        
    }
}
