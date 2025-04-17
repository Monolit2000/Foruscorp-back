using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients
{
    public interface ITruckerPathApi
    {
        Task<DataObject> PlanRouteAsync(GeoPoint origin, GeoPoint destinations, List<GeoPoint> viaPoints = null, CancellationToken cancellationToken = default);
        Task<SimpleDropPointResponse> DropPoint(double latitude, double longitude, int level = 4, int radius = 10000, CancellationToken cancellationToken = default);
    }
}
