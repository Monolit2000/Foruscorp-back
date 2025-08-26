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

            if (fuelRoute.IsSended && !fuelRoute.IsEdited)
            {
                logger.LogWarning("Fuel route with ID {RouteId} has already been sent.", request.RouteId);
                return Result.Fail("Fuel route has already been sent.");
            }

            if (fuelRoute == null)
            {
                logger.LogWarning("Fuel route with ID {RouteId} not found.", request.RouteId);
                return Result.Fail("Fuel route not found.");
            }

            var routeSection = await context.RouteSections
                .Where(fs => fs.Id == request.RouteSectionId)
                .FirstOrDefaultAsync();

            if (routeSection == null)
            {
                logger.LogWarning("Route section with ID {RouteSectionId} not found.", request.RouteSectionId);
                return Result.Fail("Route section not found.");
            }

            if (routeSection.IsAssigned)
            {
                logger.LogWarning("Route section with ID {RouteSectionId} has already been assigned.", request.RouteSectionId);
                return Result.Fail("Route section has already been assigned.");
            }

            if(fuelRoute.IsEdited)
            {
                await MurkAsEdited(fuelRoute);
            }

            fuelRoute.RouteSections.Add(routeSection);

            logger.LogInformation("Processing fuel route: {FuelRoute}", JsonSerializer.Serialize(fuelRoute, new JsonSerializerOptions { WriteIndented = true}));

            fuelRoute.Assign(request.RouteSectionId);
            //fuelRoute.MarkAsAccepted();

            await context.SaveChangesAsync(cancellationToken);

            var publishEventTask = publishEndpoint.Publish(
                new RouteAssignedIntegrationEvent(fuelRoute.Id, request.TruckId)
            );

            logger.LogInformation("Fuel route {RouteId} sent successfully.", fuelRoute.Id);
            return Result.Ok().WithSuccess("Fuel route sent successfully.");
        }

        public async Task MurkAsEdited(FuelRoute fuelRoute)
        {
            var editedSection = await context.RouteSections
                .Where(fs => fs.RouteId == fuelRoute.Id && fs.IsAssigned)
                .FirstOrDefaultAsync();

            editedSection.MarkAsEdited();

            context.RouteSections.Update(editedSection);
        }
    }
}