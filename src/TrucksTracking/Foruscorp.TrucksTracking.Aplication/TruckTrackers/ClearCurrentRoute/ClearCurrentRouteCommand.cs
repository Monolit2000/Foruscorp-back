using MediatR;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.ClearCurrentRoute
{
    public class ClearCurrentRouteCommand : IRequest
    {
        public Guid TruckId { get; set; }
    }
}
