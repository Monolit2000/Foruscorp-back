using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.IntegrationEvents
{
    //NearFuelStationFoundIntegrationEvent
    public record FuelStationPlanMarkedAsNearIntegretionEvent(Guid TruckId, Guid FuelStationId, double Longitude, double Latitude, double DistanceKm);
   
}
