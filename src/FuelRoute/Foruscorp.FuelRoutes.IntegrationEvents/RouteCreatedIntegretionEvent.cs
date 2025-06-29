using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.IntegrationEvents
{
    public class RouteCreatedIntegretionEvent 
    {
        public Guid TruckId { get; set; }   
        public Guid RouteId{ get; set; }
    }
}
