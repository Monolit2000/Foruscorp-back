using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.IntegrationEvents
{
    public class RouteAccptedIntegrationEvent
    {
        public Guid RouteId { get; set; }
        public Guid TruckId { get; set; }
        public bool IsSelfAssign { get; set; }
        public DateTime SentAt { get; set; }
        public RouteAccptedIntegrationEvent(Guid routeId, Guid truckId, bool isSelfAssign = false)
        {
            RouteId = routeId;
            TruckId = truckId;
            SentAt = DateTime.UtcNow;
            IsSelfAssign = isSelfAssign;
        }
    }
}
