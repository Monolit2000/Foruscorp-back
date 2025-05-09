using MediatR;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Aplication.Configuration.CaheKeys;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute
{
    public class AcceptFuelRouteCommandHandler(
        IMemoryCache memoryCache,
        IServiceScopeFactory scopeFactory,
        IFuelRouteContext fuelRouteContext,
        IFuelRouteRopository fuelRouteRopository) : IRequestHandler<AcceptFuelRouteCommand, Result>
    {
        public async Task<Result> Handle(AcceptFuelRouteCommand request, CancellationToken cancellationToken)
        {
            memoryCache.TryGetValue(FuelRoutesCachKeys.RouteById(request.Id), out DataObject routeDataValue);
            if (routeDataValue is null)
                return Result.Fail("Route not found");

            var section = routeDataValue.Routes.WaypointsAndShapes
                .Where(ws => ws != null && ws.Sections != null)
                .SelectMany(x => x.Sections)
                .FirstOrDefault(section => section.Id == request.RouteSectionId);

            var fuelRoute = FuelRoute.CreateNew(
                Guid.NewGuid(),
                "origin",
                "destinztion",
                new List<RouteFuelPoint>(),
                new List<MapPoint>());

            var mupPoints = section.ShowShape
                .Select(x => MapPoint.CreateNew(fuelRoute.Id, x));

            var encodedRoute = PolylineEncoder.EncodePolyline(section.ShowShape);
            fuelRoute.AddEncodedRoute(encodedRoute);

            await fuelRouteContext.FuelRoutes.AddAsync(fuelRoute, cancellationToken);
            await fuelRouteContext.SaveChangesAsync(cancellationToken);

            //await fuelRouteRopository.BulkInsertAsync(mupPoints);

            return Result.Ok();
        }
    }
}





