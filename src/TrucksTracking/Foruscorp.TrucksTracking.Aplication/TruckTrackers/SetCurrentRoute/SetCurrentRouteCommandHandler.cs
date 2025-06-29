using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTracker;
using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.SetCurrentRoute
{
    public class SetCurrentRouteCommandHandler(
        ITuckTrackingContext tuckTrackingContext,
        ILogger<UpdateTruckTrackerCommandHandler> logger) : IRequestHandler<SetCurrentRouteCommand>
    {
        public async Task Handle(SetCurrentRouteCommand request, CancellationToken cancellationToken)
        {

            var truckTracker = tuckTrackingContext.TruckTrackers
                .Include(tt => tt.Routes)
                .Include(tt => tt.CurrentRoute)
                .FirstOrDefault(t => t.TruckId == request.TruckId);

            if (truckTracker == null)
            {
                logger.LogWarning("Truck Tracker not found for TruckId: {TruckId}", request.TruckId);
                return;
            }

            //var route = Route.CreateNew(request.RouteId, truckTracker.Id, truckTracker.TruckId);

            //await tuckTrackingContext.Routes.AddAsync(route);    

            truckTracker.SetCurrentRoute(request.RouteId);

            await tuckTrackingContext.SaveChangesAsync();
        }
    }
}
