using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AssignRoute
{

    public record AssignRouteCommand(Guid RouteId, Guid TruckId) : IRequest<Result>;

    public class AssignRouteCommandHandler(
        IPublishEndpoint publishEndpoint, 
        IFuelRouteContext context) : IRequestHandler<AssignRouteCommand, Result>
    {
        public async Task<Result> Handle(AssignRouteCommand request, CancellationToken cancellationToken)
        {
            var fuelRoute = context.FuelRoutes
                .FirstOrDefault(x => x.Id == request.RouteId);

            if (fuelRoute == null)
                return Result.Fail("Fuel route not found.");

            fuelRoute.MarkAsSended();

            await context.SaveChangesAsync(cancellationToken);

            await publishEndpoint.Publish(new RouteAssignedIntegrationEvent(request.RouteId, request.TruckId));

            return Result.Ok().WithSuccess("Fuel route sent successfully.");    
        }
    }
}
