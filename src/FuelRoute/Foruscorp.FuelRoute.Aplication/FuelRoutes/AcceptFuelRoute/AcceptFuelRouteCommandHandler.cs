using MediatR;
using FluentResults;
using Microsoft.Extensions.Caching.Memory;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Aplication.Configuration.CaheKeys;
using Microsoft.AspNetCore.Components.Forms;
using System.Data.Entity;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute
{
    public class AcceptFuelRouteCommandHandler(
        IMemoryCache memoryCache,
        IFuelRouteContext fuelRouteContext,
        IFuelRouteRopository fuelRouteRopository) : IRequestHandler<AcceptFuelRouteCommand, Result>
    {
        public async Task<Result> Handle(AcceptFuelRouteCommand request, CancellationToken cancellationToken)
        {
            var reoute = await fuelRouteContext.FuelRoutes
                .FirstOrDefaultAsync(r => r.Id == request.RouteId, cancellationToken);

            if (reoute == null)
                return Result.Fail("Route not found");

            reoute.MarkAsAccepted();

            await fuelRouteContext.SaveChangesAsync(cancellationToken);
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