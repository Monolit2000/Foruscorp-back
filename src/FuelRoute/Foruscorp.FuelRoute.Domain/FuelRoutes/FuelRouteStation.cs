using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public class FuelRouteStation
    {
        public Guid FuelStationId { get; private set; }    

        public Guid Id { get; private set; }    

        public bool IsFueled { get; private set; }  
    }
}
