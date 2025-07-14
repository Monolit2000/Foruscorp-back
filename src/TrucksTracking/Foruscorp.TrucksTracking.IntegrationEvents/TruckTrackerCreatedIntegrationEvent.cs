using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.IntegrationEvents
{
    public record TruckTrackerCreatedIntegrationEvent(Guid TruckId, Guid TruckTrackerId, string ProviderTruckId);
   
}
