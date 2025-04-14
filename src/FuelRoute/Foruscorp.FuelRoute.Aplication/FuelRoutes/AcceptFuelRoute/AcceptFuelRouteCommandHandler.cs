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
                .FirstOrDefault(x => x.Id == request.RouteSectionId);

            var fuelRoute = FuelRoute.CreateNew(
                Guid.NewGuid(),
                "origin",
                "destinztion",
                new List<RouteFuelPoint>(),
                new List<MapPoint>());

            var mupPoints = section.ShowShape
                .Select(x => MapPoint.CreateNew(fuelRoute.Id, x));

            await fuelRouteContext.FuelRoutes.AddAsync(fuelRoute, cancellationToken);
            await fuelRouteContext.SaveChangesAsync(cancellationToken);

            await fuelRouteRopository.BulkInsertAsync(mupPoints);

            return Result.Ok();
        }
    }
}









//const int chunkSize = 1000;
//var partitionedPoints = mupPoints
//    .Select((point, index) => new { Point = point, Index = index })
//    .GroupBy(x => x.Index / chunkSize)
//    .Select(g => g.Select(x => x.Point).ToList())
//    .ToList();

//// Process chunks in parallel
//await Task.WhenAll(partitionedPoints.Select(async chunk =>
//{
//    using var scope = scopeFactory.CreateScope();
//    var context = scope.ServiceProvider.GetRequiredService<IFuelRouteContext>();
//    await context.MapPoints.AddRangeAsync(chunk, cancellationToken);
//    await context.SaveChangesAsync(cancellationToken);
//}));



//_ = Task.Run(async () =>
//{
//    //try
//    //{
//        using var scope = scopeFactory.CreateScope();
//        var context = scope.ServiceProvider.GetRequiredService<IFuelRouteContext>();
//        Console.WriteLine("TEST");
//        await context.FuelRoutes.AddAsync(fuelRoute, cancellationToken);
//        await context.SaveChangesAsync(cancellationToken);
//        Console.WriteLine("TEST");
//    //}
//    //catch (Exception ex)
//    //{
//    //    // TODO: логирование ошибки, например:
//    //    Console.WriteLine($"Failed to save fuel route: {ex.Message}");
//    //}
//});