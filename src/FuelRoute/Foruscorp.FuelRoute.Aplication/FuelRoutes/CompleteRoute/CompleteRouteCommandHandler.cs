using DocumentFormat.OpenXml.InkML;
using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.CompleteRoute
{
    public record CompleteRouteCommand(Guid RouteId) : IRequest<Result>;
    public class CompleteRouteCommandHandler(
        IPublishEndpoint publishEndpoint,
        IFuelRouteContext context,
        ILogger<CompleteRouteCommandHandler> logger) : IRequestHandler<CompleteRouteCommand, Result>

    {
        public async Task<Result> Handle(CompleteRouteCommand request, CancellationToken cancellationToken)
        {
            var fuelRoute = await context.FuelRoutes
                .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if (fuelRoute == null)
            {
                logger.LogWarning("Fuel route with ID {RouteId} not found", request.RouteId);
                return Result.Fail($"Fuel route with ID {request.RouteId} not found");
            }

            if (!fuelRoute.IsAccepted)
            {
                logger.LogWarning("Fuel route with ID {RouteId} is not accepted yet", request.RouteId);
                return Result.Fail($"Fuel route with ID {request.RouteId} is not accepted yet");
            }

            if (fuelRoute.IsComplet)
            {
                logger.LogWarning("Fuel route with ID {RouteId} is already completed", request.RouteId);
                return Result.Fail($"Fuel route with ID {request.RouteId} is already completed");
            }

            try
            {
                // Mark the route as completed
                fuelRoute.CompleteRoute();
                
                // Update the database
                context.FuelRoutes.Update(fuelRoute);
                await context.SaveChangesAsync(cancellationToken);

                // Publish integration event for route completion
                await publishEndpoint.Publish(new RouteCompletedIntegrationEvent
                {
                    RouteId = fuelRoute.Id,
                    TruckId = fuelRoute.TruckId,
                    CompletedAt = DateTime.UtcNow
                }, cancellationToken);

                logger.LogInformation("Fuel route {RouteId} completed successfully", request.RouteId);
                return Result.Ok().WithSuccess($"Fuel route {request.RouteId} completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error completing fuel route {RouteId}", request.RouteId);
                return Result.Fail($"Error completing fuel route: {ex.Message}");
            }
        }
    }
}
