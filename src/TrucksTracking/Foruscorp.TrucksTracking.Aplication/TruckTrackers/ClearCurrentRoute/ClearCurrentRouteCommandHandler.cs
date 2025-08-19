using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.ClearCurrentRoute
{
    public class ClearCurrentRouteCommandHandler(
        ITruckTrackingContext truckTrackingContext,
        ILogger<ClearCurrentRouteCommandHandler> logger) : IRequestHandler<ClearCurrentRouteCommand>
    {
        public async Task Handle(ClearCurrentRouteCommand request, CancellationToken cancellationToken)
        {
            var truckTracker = truckTrackingContext.TruckTrackers
                .Include(tt => tt.CurrentRoute)
                .FirstOrDefault(t => t.TruckId == request.TruckId);

            if (truckTracker == null)
            {
                logger.LogWarning("Truck Tracker not found for TruckId: {TruckId}", request.TruckId);
                return;
            }

            truckTracker.ClearCurrentRoute();

            await truckTrackingContext.SaveChangesAsync(cancellationToken);
            
            logger.LogInformation("Current route cleared for TruckId: {TruckId}", request.TruckId);
        }
    }
}
