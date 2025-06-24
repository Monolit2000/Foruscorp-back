using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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

        [NotMapped]
        public string RouteSectionResponceId { get; set; } 

        public FuelRouteSection() { } 

        public FuelRouteSection(Guid routeId, string encodeRoute)
        {
            Id = Guid.NewGuid();
            RouteId = routeId; 
            EncodeRoute = encodeRoute;  
        }

        public FuelRouteSection(Guid routeId, string routeSectionResponceId, string encodeRoute)
        {
            Id = Guid.NewGuid();
            RouteId = routeId;
            EncodeRoute = encodeRoute;
            RouteSectionResponceId = routeSectionResponceId;
        }


    }
}
