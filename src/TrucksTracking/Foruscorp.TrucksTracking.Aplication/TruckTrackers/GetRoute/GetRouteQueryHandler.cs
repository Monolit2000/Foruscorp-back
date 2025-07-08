using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Domain.Trucks;
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
                .Include(tt => tt.CurrentTruckLocation)
                .AsNoTracking()
                .FirstOrDefault(tt => tt.TruckId == request.TruckId);

            if (truckTracker == null)
            {
                logger.LogWarning($"Truck Tracker not found for TruckId: {request.TruckId}");
                return new RouteDto
                {
                    CurrentLocation = null,
                    RouteId = null,
                    MapPoints = new List<double[]>()
                };
            }

            var route = truckTracker.CurrentRoute;

            if (route == null)
            {
                return new RouteDto
                {
                    CurrentLocation = truckTracker.CurrentTruckLocation?.Location,
                    RouteId = null,
                    MapPoints = new List<double[]>()
                };
            }

            var mapPoints = await tuckTrackingContext.TruckLocations
                .Where(tl => tl.RouteId == route.RouteId)
                .OrderBy(tl => tl.RecordedAt)
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
        }
    }

    public class RouteDto
    {
        public bool IsRoute { get; set; }
        public GeoPoint CurrentLocation { get; set; }
        public Guid? RouteId { get; set; }
        public List<double[]> MapPoints { get; set; }
    }
}
