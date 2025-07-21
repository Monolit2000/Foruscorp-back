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
        public RouteSectionInfo RouteSectionInfo { get; private set; }

        public int RouteVersion { get; set; } = 0; 

        public decimal FuelNeeded { get; set; } = 0.0m;

        public bool IsAssigned { get; private set; } 

        public List<FuelRouteStation> FuelRouteStations = [];

        [NotMapped]
        public string RouteSectionResponceId { get; set; } 

        public FuelRouteSection() { } 

        public FuelRouteSection(Guid routeId, string encodeRoute)
        {
            Id = Guid.NewGuid();
            RouteId = routeId; 
            EncodeRoute = encodeRoute;
            IsAssigned = false; 
        }

        public FuelRouteSection(Guid routeId, string routeSectionResponceId, string encodeRoute)
        {
            //Id = Guid.NewGuid();
            RouteId = routeId;
            EncodeRoute = encodeRoute;
            RouteSectionResponceId = routeSectionResponceId;
        }

        public RouteSectionInfo SetRouteSectionInfo(double Tolls, double Gallons, double Miles, int DriveTime)
        {
            var routeSectionInfo = new RouteSectionInfo(Tolls, Gallons, Miles, DriveTime);
            RouteSectionInfo = routeSectionInfo;
            return RouteSectionInfo;    
        }
        public void MarkAsAssigned()
        {
            IsAssigned = true;
        }
        public void MarkAsUnassigned()
        {
            IsAssigned = false;
        }
    }
}
