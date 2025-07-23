using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.IntegrationEvents
{
    public record DriverNearFuelStationIntegrationEvent(Guid UserId, Guid FuelStationId, string Address, double DistanceKm);
}
