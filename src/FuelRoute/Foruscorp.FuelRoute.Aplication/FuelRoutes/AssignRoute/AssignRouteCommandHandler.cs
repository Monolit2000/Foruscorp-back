﻿using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

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
                  .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if (fuelRoute == null)
                return Result.Fail("Fuel route not found.");

            fuelRoute.MarkAsSended(request.RouteSectionId);

            await context.SaveChangesAsync(cancellationToken);

            await publishEndpoint.Publish(new RouteAssignedIntegrationEvent(fuelRoute.Id, request.TruckId));

            return Result.Ok().WithSuccess("Fuel route sent successfully.");    
        }
    }
}
