using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.Contructs.WebScrapers
{
    public interface ITheFuelMapScraper
    {
        public Task<string> GetBearerToken();    
    }
}
