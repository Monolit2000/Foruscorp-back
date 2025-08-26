using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.Json;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AssignRoute
{
    public record AssignRouteCommand(Guid RouteId, Guid RouteSectionId, Guid TruckId) : IRequest<Result>;

    public class AssignRouteCommandHandler(
        IPublishEndpoint publishEndpoint,
        IFuelRouteContext context,
        ILogger<AssignRouteCommandHandler> logger) : IRequestHandler<AssignRouteCommand, Result>
    {
        public async Task<Result> Handle(AssignRouteCommand request, CancellationToken cancellationToken)
        {
            var fuelRoute = await context.FuelRoutes
                .Include(x => x.FuelRouteStations.Where(fs => fs.RoadSectionId == request.RouteSectionId))
                .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if (fuelRoute.IsSended)
            {
                logger.LogWarning("Fuel route with ID {RouteId} has already been sent.", request.RouteId);
                return Result.Fail("Fuel route has already been sent.");
            }

            if (fuelRoute == null)
            {
                logger.LogWarning("Fuel route with ID {RouteId} not found.", request.RouteId);
                return Result.Fail("Fuel route not found.");
            }

            var routeSections = await context.RouteSections
                .Where(fs => fs.Id == request.RouteSectionId)
                .ToListAsync();

            fuelRoute.RouteSections = routeSections;

            logger.LogInformation("Processing fuel route: {FuelRoute}", JsonSerializer.Serialize(fuelRoute, new JsonSerializerOptions { WriteIndented = true}));

            fuelRoute.Assign(request.RouteSectionId);
            //fuelRoute.MarkAsAccepted();

            await context.SaveChangesAsync(cancellationToken);

            var publishEventTask = publishEndpoint.Publish(
                new RouteAssignedIntegrationEvent(fuelRoute.Id, request.TruckId)
            );

            //var fuelRouteStations = fuelRoute.FuelRouteStations
            //    .Where(fs => fs.RoadSectionId == request.RouteSectionId && fs.IsAlgorithm)
            //    .Select(fs => new FuelRouteStationPlan(
            //        fs.FuelStationId,
            //        fs.FuelRouteId,
            //        request.TruckId,
            //        fs.Address,
            //        16.0,
            //        double.Parse(fs.Refill, CultureInfo.InvariantCulture),
            //        double.Parse(fs.Longitude, CultureInfo.InvariantCulture),
            //        double.Parse(fs.Latitude, CultureInfo.InvariantCulture)))
            //    .ToList();

            //var publishStationsTask = PublisFuelStationsPlan(fuelRouteStations);

            //await Task.WhenAll(publishEventTask, publishStationsTask);

            logger.LogInformation("Fuel route {RouteId} sent successfully.", fuelRoute.Id);
            return Result.Ok().WithSuccess("Fuel route sent successfully.");
        }
    }
}