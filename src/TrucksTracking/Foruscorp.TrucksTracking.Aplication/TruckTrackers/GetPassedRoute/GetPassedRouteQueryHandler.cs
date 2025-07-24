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

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.GetPassedRoute
{
    public class GetPassedRouteQueryHandler(
        ITruckTrackingContext tuckTrackingContext,
        ILogger<GetPassedRouteQueryHandler> logger) : IRequestHandler<GetPassedRouteQuery, RouteDto>
    {
        public async Task<RouteDto> Handle(GetPassedRouteQuery request, CancellationToken cancellationToken)
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
                    MapPoints = new List<CoordinateDto>(),
                };
            }

            var route = truckTracker.CurrentRoute;

            if (route == null)
            {
                return new RouteDto
                {
                    CurrentLocation = truckTracker.CurrentTruckLocation?.Location,
                    RouteId = null,
                    MapPoints = new List<CoordinateDto>(),
                    FormattedLocation = truckTracker.CurrentTruckLocation.FormattedLocation
                };
            }

            var mapPoints = await tuckTrackingContext.TruckLocations
                .Where(tl => tl.RouteId == route.RouteId)
                .OrderBy(tl => tl.RecordedAt)
                .AsNoTracking()
                .Select(tl => new CoordinateDto
                {
                    Longitude = tl.Location.Longitude,
                    Latitude = tl.Location.Latitude
                })
                .ToListAsync(cancellationToken);

            return new RouteDto
            {
                CurrentLocation = truckTracker.CurrentTruckLocation?.Location,
                FormattedLocation = truckTracker.CurrentTruckLocation.FormattedLocation,
                IsRoute = true,
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
        public List<CoordinateDto> MapPoints { get; set; }
        public string FormattedLocation { get; set; } = string.Empty;   
    }

    public class CoordinateDto
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }

}
