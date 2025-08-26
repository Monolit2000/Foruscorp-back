using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Configuration.CaheKeys;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MassTransit.Transports;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Globalization;
using static Foruscorp.FuelRoutes.Aplication.FuelRoutes.AssignRoute.AssignRouteCommandHandler;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute
{
    public class AcceptFuelRouteCommandHandler(
        IMemoryCache memoryCache,
        IPublishEndpoint publishEndpoint,
        IFuelRouteContext fuelRouteContext,
        IFuelRouteRopository fuelRouteRopository,
        ILogger<AcceptFuelRouteCommandHandler> logger) : IRequestHandler<AcceptFuelRouteCommand, Result>
    {
        public async Task<Result> Handle(AcceptFuelRouteCommand request, CancellationToken cancellationToken)
        {
            var route = await fuelRouteContext.FuelRoutes
                .FirstOrDefaultAsync(r => r.Id == request.RouteId, cancellationToken);

            if (route == null)
                return Result.Fail("Route not found");

            if (route.IsAccepted && !route.IsEdited)
            {
                logger.LogWarning("Fuel route with ID {RouteId} has already been accepted.", request.RouteId);
                return Result.Fail("Fuel route has already been accepted.");
            }

            route.MarkAsAccepted();

            var routSection = await fuelRouteContext.RouteSections
                .FirstOrDefaultAsync(rs => rs.RouteId == route.Id && (rs.IsAssigned && !rs.IsEdited));

            routSection.MarkAsAssigned();

             fuelRouteContext.RouteSections
                .RemoveRange(fuelRouteContext.RouteSections.Where(rs => rs.RouteId == route.Id && rs.Id != routSection.Id));  

            await fuelRouteContext.SaveChangesAsync(cancellationToken);

            var publishEventTask = publishEndpoint.Publish(
                    new RouteAccptedIntegrationEvent(route.Id, route.TruckId)
                );

            var fuelRouteStations = await fuelRouteContext.FuelRouteStation
                .Where(fs => fs.RoadSectionId == routSection.Id && fs.IsAlgorithm)
                .Select(fs => new FuelRouteStationPlan(
                    fs.FuelStationId,
                    fs.FuelRouteId,
                    route.TruckId,
                    fs.Address,
                    16.0,
                    double.Parse(fs.Refill, CultureInfo.InvariantCulture),
                    double.Parse(fs.Longitude, CultureInfo.InvariantCulture),
                    double.Parse(fs.Latitude, CultureInfo.InvariantCulture)))
                .ToListAsync();

            var publishStationsTask = PublisFuelStationsPlan(fuelRouteStations);

            await Task.WhenAll(publishEventTask, publishStationsTask);

            return Result.Ok();
        }

        private async Task PublisFuelStationsPlan(List<FuelRouteStationPlan> fuelRouteStations)
        {
            foreach (var planedStation in fuelRouteStations)
            {
                await publishEndpoint.Publish(new FuelStationPlanAssignedIntegrationEvent(
                    planedStation.FuelStationId,
                    planedStation.RouteId,
                    planedStation.TruckId,
                    planedStation.Address,
                    planedStation.nearDistance,
                    planedStation.refill,
                    planedStation.Longitude,
                    planedStation.Latitude));
            }
        }

        public record FuelRouteStationPlan(
            Guid FuelStationId,
            Guid RouteId,
            Guid TruckId,
            string Address,
            double nearDistance,
            double refill,
            double Longitude,
            double Latitude);
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