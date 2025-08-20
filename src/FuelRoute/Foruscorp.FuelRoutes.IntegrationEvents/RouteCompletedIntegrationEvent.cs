using System;

namespace Foruscorp.FuelRoutes.IntegrationEvents
{
    public class RouteCompletedIntegrationEvent
    {
        public Guid RouteId { get; set; }
        public Guid TruckId { get; set; }
        public DateTime CompletedAt { get; set; }
    }
}
