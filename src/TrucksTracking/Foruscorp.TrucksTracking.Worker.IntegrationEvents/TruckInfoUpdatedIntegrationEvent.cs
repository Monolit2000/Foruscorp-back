using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Worker.IntegrationEvents
{
    public sealed record TruckInfoUpdatedIntegrationEvent(
    string TruckId,
    string TruckName,
    double Longitude,
    double Latitude,
    string Time,
    double HeadingDegrees,
    double fuelPercents,
    string formattedLocation,
    string engineStateTime,
    string engineStateValue);


    public sealed record TruckInfoUpdatedIntegrationEvents(List<TruckInfoUpdatedIntegrationEvent> truckInfoUpdatedIntegrationEvents);
}
