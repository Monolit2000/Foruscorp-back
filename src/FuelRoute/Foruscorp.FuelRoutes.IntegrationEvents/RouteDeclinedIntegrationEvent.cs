using System;

namespace Foruscorp.FuelRoutes.IntegrationEvents
{
    public class RouteDeclinedIntegrationEvent
    {
        public Guid RouteId { get; set; }
        public Guid TruckId { get; set; }
        public DateTime DeclinedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
    }
}
