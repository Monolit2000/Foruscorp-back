using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.IntegrationEvents
{
    public record FuelStationPlanAssignedIntegrationEvent(
        Guid FuelStationId, 
        Guid RouteId, 
        Guid TruckId, 
        string Address,
        double NearDistance,
        double refill,
        double Longitude, 
        double Latitude);
}
