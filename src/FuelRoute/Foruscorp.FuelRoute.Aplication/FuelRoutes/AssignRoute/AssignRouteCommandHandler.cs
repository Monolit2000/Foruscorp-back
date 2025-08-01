﻿using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AssignRoute
{

    public record AssignRouteCommand(Guid RouteId, Guid RouteSectionId, Guid TruckId) : IRequest<Result>;

    public class AssignRouteCommandHandler(
        IPublishEndpoint publishEndpoint, 
        IFuelRouteContext context) : IRequestHandler<AssignRouteCommand, Result>
    {
        public async Task<Result> Handle(AssignRouteCommand request, CancellationToken cancellationToken)
        {
            var fuelRoute = await context.FuelRoutes
                  .Include(x => x.RouteSections)
                  .Include(x => x.FuelRouteStations.Where(fs => fs.RoadSectionId == request.RouteSectionId))    
                  .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if (fuelRoute == null)
                return Result.Fail("Fuel route not found.");

            fuelRoute.MarkAsSended(request.RouteSectionId);

            fuelRoute.MarkAsAccepted();

            await context.SaveChangesAsync(cancellationToken);

            await publishEndpoint.Publish(new RouteAssignedIntegrationEvent(fuelRoute.Id, request.TruckId));

            var fuelRouteStations = fuelRoute.FuelRouteStations
                .Where(fs => fs.RoadSectionId == request.RouteSectionId && fs.IsAlgorithm)
                .Select(fs => new FuelRouteStationPlan(
                    fs.FuelStationId,
                    fs.FuelRouteId,
                    request.TruckId,
                    fs.Address,
                    16.0,
                    double.Parse(fs.Longitude, CultureInfo.InvariantCulture),
                    double.Parse(fs.Latitude, CultureInfo.InvariantCulture)))
                .ToList();

            await PublisFuelStationsPlan(fuelRouteStations);

            return Result.Ok().WithSuccess("Fuel route sent successfully.");    
        }

        private async Task PublisFuelStationsPlan(List<FuelRouteStationPlan> fuelRouteStations)
        {
            foreach(var planedStation in fuelRouteStations)
            {
                await publishEndpoint.Publish(new FuelStationPlanAssignedIntegrationEvent(
                    planedStation.FuelStationId,
                    planedStation.RouteId,
                    planedStation.TruckId,
                    planedStation.Address,
                    planedStation.nearDistance, 
                    planedStation.Longitude,
                    planedStation.Latitude));
            }
        }

        public record FuelRouteStationPlan(Guid FuelStationId, Guid RouteId, Guid TruckId, string Address, double nearDistance, double Longitude, double Latitude);
    }
}
