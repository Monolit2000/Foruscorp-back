using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public class FuelRouteSection
    {
        public Guid Id { get; private set; }
        public Guid RouteId { get; private set; }
        public string EncodeRoute { get; private set; } // Base64 encoded route

        public List<FuelRouteStation> FuelRouteStations = [];


        public FuelRouteSection() { } 

        public FuelRouteSection(Guid routeId, string encodeRoute)
        {
            Id = Guid.NewGuid();
            RouteId = routeId; 
            EncodeRoute = encodeRoute;  
        }
    }
}
