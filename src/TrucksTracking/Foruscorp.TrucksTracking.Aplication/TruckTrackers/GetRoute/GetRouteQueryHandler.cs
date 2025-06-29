using Foruscorp.TrucksTracking.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.GetRoute
{
    public class GetRouteQueryHandler(
        ITuckTrackingContext tuckTrackingContext,
        ILogger<GetRouteQueryHandler> logger) : IRequestHandler<GetRouteQuery, RouteDto>
    {
        public async Task<RouteDto> Handle(GetRouteQuery request, CancellationToken cancellationToken)
        {
            var truckTracker = tuckTrackingContext.TruckTrackers
                .Include(tt => tt.CurrentRoute)
                .FirstOrDefault(tt => tt.TruckId == request.TruckId);

            if(truckTracker == null)
                throw new InvalidOperationException($"Truck Tracker not found for TruckId: {request.TruckId}");

            var route = truckTracker.CurrentRoute;

            if (route == null)
                throw new InvalidOperationException($"No current route set for TruckId: {request.TruckId}");

            var mapPoints = await tuckTrackingContext.TruckLocations
                .Where(tl => tl.RouteId == route.RouteId)
                .OrderByDescending(tl => tl.RecordedAt)
                .Select(tl => new[]
                {
                    tl.Location.Latitude,
                    tl.Location.Longitude
                })
                .ToListAsync(cancellationToken);

            return new RouteDto
            {
                RouteId = route.RouteId,
                MapPoints = mapPoints
            };

            throw new NotImplementedException();
        }
    }

    public class RouteDto
    {
        public Guid RouteId { get; set; }
        public List<double[]> MapPoints { get; set; }
    }
}
