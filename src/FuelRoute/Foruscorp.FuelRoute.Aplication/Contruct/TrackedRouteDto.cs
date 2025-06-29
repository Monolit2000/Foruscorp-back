using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.Contruct
{
    public class TrackedRouteDto
    {
        public Guid RouteId { get; set; }
        public List<double[]> MapPoints { get; set; }
    }
}
