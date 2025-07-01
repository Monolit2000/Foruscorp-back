using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.Contruct
{
    public class TrackedRouteDto
    {
        public bool IsRoute {get; set;}
        public GeoPoint CurrentLocation { get; set; }
        public Guid RouteId { get; set; }
        public List<double[]> MapPoints { get; set; }
    }
}
