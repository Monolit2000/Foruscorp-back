using MediatR;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Aplication.Configuration.CaheKeys;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute
{
    public class AcceptFuelRouteCommandHandler(
        IMemoryCache memoryCache) : IRequestHandler<AcceptFuelRouteCommand, Result>
    {
        public async Task<Result> Handle(AcceptFuelRouteCommand request, CancellationToken cancellationToken)
        {
            memoryCache.TryGetValue(FuelRoutesCachKeys.RouteById(request.Id), out DataObject routeDataValue);
            if (routeDataValue is null)
                return Result.Fail("Route not found");

            var section = routeDataValue.Routes.WaypointsAndShapes
                .SelectMany(x => x.Sections)
                .FirstOrDefault(x => x.Id == request.RouteSectionId);

            return section is not null
                ? Result.Ok()
                : Result.Fail("Route section not found");   
        }
    }
}
