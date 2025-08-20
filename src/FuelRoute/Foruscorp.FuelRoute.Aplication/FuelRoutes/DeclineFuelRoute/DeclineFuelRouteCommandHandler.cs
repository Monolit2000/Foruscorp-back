using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.DeclineFuelRoute
{
    public record DeclineFuelRouteCommand(Guid RouteId, string Reason = "") : IRequest<Result>;
    
    public class DeclineFuelRouteCommandHandler(
        IPublishEndpoint publishEndpoint,
        IFuelRouteContext context,
        ILogger<DeclineFuelRouteCommandHandler> logger) : IRequestHandler<DeclineFuelRouteCommand, Result>
    {
        public async Task<Result> Handle(DeclineFuelRouteCommand request, CancellationToken cancellationToken)
        {
            var fuelRoute = await context.FuelRoutes
                .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if (fuelRoute == null)
            {
                logger.LogWarning("Fuel route with ID {RouteId} not found", request.RouteId);
                return Result.Fail($"Fuel route with ID {request.RouteId} not found");
            }

            if (fuelRoute.IsComplet)
            {
                logger.LogWarning("Fuel route with ID {RouteId} is already completed and cannot be declined", request.RouteId);
                return Result.Fail($"Fuel route with ID {request.RouteId} is already completed and cannot be declined");
            }

            if (fuelRoute.IsDeclined)
            {
                logger.LogWarning("Fuel route with ID {RouteId} is already declined", request.RouteId);
                return Result.Fail($"Fuel route with ID {request.RouteId} is already declined");
            }

            try
            {
                // Mark the route as declined using the domain method
                fuelRoute.DeclineRoute();
                
                // Update the database
                context.FuelRoutes.Update(fuelRoute);
                await context.SaveChangesAsync(cancellationToken);

                // Publish integration event for route decline
                await publishEndpoint.Publish(new RouteDeclinedIntegrationEvent
                {
                    RouteId = fuelRoute.Id,
                    TruckId = fuelRoute.TruckId,
                    DeclinedAt = DateTime.UtcNow,
                    Reason = request.Reason
                }, cancellationToken);

                logger.LogInformation("Fuel route {RouteId} declined successfully", request.RouteId);
                return Result.Ok().WithSuccess($"Fuel route {request.RouteId} declined successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error declining fuel route {RouteId}", request.RouteId);
                return Result.Fail($"Error declining fuel route: {ex.Message}");
            }
        }
    }
}
