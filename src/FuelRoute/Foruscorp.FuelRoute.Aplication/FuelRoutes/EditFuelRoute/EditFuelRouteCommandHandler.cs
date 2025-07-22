using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Configuration.GeoTools;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json.Serialization;
using static Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute.CreateFuelRouteCommandHandler;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.EditFuelRoute
{
    public class EditFuelRouteCommand() : IRequest<Result<FuelRouteDto>>
    {
        public Guid TruckId { get; set; }
        public string OriginName { get; set; } = "OriginName";
        public string DestinationName { get; set; } = "DestinationName";
        public GeoPoint Origin { get; set; }
        public GeoPoint Destination { get; set; }

        public double Weight { get; set; } = 40000.0; // in Paunds

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GeoPoint> ViaPoints { get; set; }

    }

    public class EditFuelRouteCommandHandler(
        IFuelRouteContext fuelRouteContext,
        ITruckerPathApi truckerPathApi,
        IMemoryCache memoryCache,
        ISender sender,
        IPublishEndpoint publishEndpoint,
        ITruckTrackingService truckClient) : IRequestHandler<EditFuelRouteCommand, Result<FuelRouteDto>>
    {

        public double POINT_RADIUS_KM = 8.0;

        public async Task<Result<FuelRouteDto>> Handle(EditFuelRouteCommand request, CancellationToken cancellationToken)
        {
            var route = await truckClient.GetRouteAsync(request.TruckId);

            if (route == null)
                return Result.Fail("Truck route not found.");

            var fuelRoute = await fuelRouteContext.FuelRoutes
                  .Include(x => x.OriginLocation)
                  .Include(x => x.DestinationLocation)
                  //.Include(x => x.FuelRouteStations.Where(frs => !frs.IsOld))
                  .Include(x => x.RouteSections)
                  .FirstOrDefaultAsync(x => x.Id == route.RouteId, cancellationToken);


            //if (request.ViaPoints != null && request.ViaPoints.Any())
            //{
            //    request.ViaPoints = OrderViaPointsByLatitude(request.ViaPoints, request.Origin, request.Destination);
            //}

            var origin = new GeoPoint(request.Origin.Latitude, request.Origin.Longitude);
            var destinations = new GeoPoint(request.Destination.Latitude, request.Destination.Longitude);

            if (request.ViaPoints != null && request.ViaPoints.First().Latitude == 0)
                request.ViaPoints = null;

            var result = await truckerPathApi.PlanRouteAsync(origin, destinations, request.ViaPoints, cancellationToken: cancellationToken);

            if (result == null)
                return Result.Fail("ivalid route");

            //memoryCache.Set(FuelRoutesCachKeys.RouteById(result.Id), result, TimeSpan.FromHours(2));

            var sections = result.Routes.WaypointsAndShapes
                .Where(ws => ws != null && ws.Sections != null)
                .SelectMany(x => x.Sections)
                .Select(s => new
                {
                    FilteFilterPointsByDistance = GeoUtils.FilterPointsByDistance(s.ShowShape, POINT_RADIUS_KM),
                    RouteDto = new RouteDto
                    {
                        RouteSectionId = s.Id,
                        MapPoints = s.ShowShape,
                        RouteInfo = ExtractRouteSectionInfo(s)
                    }
                }).ToList();

            //var fuelRoute = FuelRoute.CreateNew(
            //    request.TruckId,
            //    LocationPoint.CreateNew(request.OriginName, request.Origin.Latitude, request.Origin.Longitude),
            //    LocationPoint.CreateNew(request.DestinationName, request.Destination.Latitude, request.Destination.Longitude),
            //    new List<FuelRouteStation>(),
            //    new List<MapPoint>(),
            //    request.Weight);


            var routeSections = sections
             .Select(x => new
             {
                 RouteSectionId = x.RouteDto.RouteSectionId,
                 EncodedRoute = PolylineEncoder.EncodePolyline(x.RouteDto.MapPoints)
             })
             .Select(x => new FuelRouteSection(fuelRoute.Id, x.RouteSectionId, x.EncodedRoute)).ToList();


            foreach (var section in sections)
            {
                var matchingRouteSection = routeSections.FirstOrDefault(rs => rs.RouteSectionResponceId == section.RouteDto.RouteSectionId);
                if (matchingRouteSection != null)
                {
                    var oldRouteSectionId = section.RouteDto.RouteSectionId;

                    section.RouteDto.RouteSectionId = matchingRouteSection.Id.ToString();

                    routeSections.FirstOrDefault(rs => rs.RouteSectionResponceId == oldRouteSectionId).SetRouteSectionInfo(
                        section.RouteDto.RouteInfo.Tolls,
                        section.RouteDto.RouteInfo.Gallons,
                        section.RouteDto.RouteInfo.Miles,
                        section.RouteDto.RouteInfo.DriveTime);
                }
            }

            var newOrigin = LocationPoint.CreateNew(request.OriginName, request.Origin.Latitude, request.Origin.Longitude);
            var newDestination = LocationPoint.CreateNew(request.DestinationName, request.Destination.Latitude, request.Destination.Longitude);

            fuelRoute.EditRoute(routeSections,
                newOrigin, 
                newDestination);

            await fuelRouteContext.SaveChangesAsync(cancellationToken);

            return new FuelRouteDto
            {
                RouteId = fuelRoute.Id.ToString(),
                RouteDtos = sections.Select(s => s.RouteDto).ToList(),
            };

        }

        public static List<GeoPoint> OrderViaPointsByLatitude(
            IEnumerable<GeoPoint>? viaPoints,
            GeoPoint origin,
            GeoPoint destination)
        {
            if (viaPoints == null)
                return new List<GeoPoint>();

            bool ascending = origin.Latitude <= destination.Latitude;

            var inRange = viaPoints.Where(p =>
                ascending
                    ? p.Latitude >= origin.Latitude && p.Latitude <= destination.Latitude
                    : p.Latitude <= origin.Latitude && p.Latitude >= destination.Latitude
            );

            var sorted = ascending
                ? inRange.OrderBy(p => p.Latitude)
                : inRange.OrderByDescending(p => p.Latitude);

            return sorted.ToList();
        }

        public RouteInfo ExtractRouteSectionInfo(RouteSection section)
        {
            if (section == null)
                return null;

            double miles = section.Summary?.Length ?? 0;
            int driveTime = section.Summary?.Duration ?? 0;

            double tolls = section.Tolls?.Count ?? 0;

            var routeInfo = new RouteInfo(tolls, 0.0, miles, driveTime);

            return routeInfo;
        }
    }
}
