using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Configuration.CaheKeys;
using Foruscorp.FuelRoutes.Aplication.Configuration.GeoTools;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.IntegrationEvents;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using MassTransit;
using MassTransit.Transports;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using FuelStationDto = Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.FuelStationDto;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute
{
    public class CreateFuelRouteCommandHandler(
        IFuelRouteContext fuelRouteContext,
        ITruckerPathApi truckerPathApi,
        IMemoryCache memoryCache,
        ISender sender,
        IPublishEndpoint publishEndpoint) : IRequestHandler<CreateFuelRouteCommand, Result<FuelRouteDto>>
    {
        private record RoutePoints(string RouteSectionId, List<List<double>> MapPoints);

        public record RouteInfo(double Tolls, double Gallons, double Miles, int DriveTime);

        public double POINT_RADIUS_KM = 8.0;

        public async Task<Result<FuelRouteDto>> Handle(CreateFuelRouteCommand request, CancellationToken cancellationToken)
        {

            if (request.TruckId == default)
                throw new ArgumentException("TruckId cannot be default value", nameof(request.TruckId));

            //if (request.ViaPoints != null && request.ViaPoints.Any())
            //{
            //    request.ViaPoints = OrderViaPointsByLatitude(request.ViaPoints, request.Origin, request.Destination);
            //}

            DataObject result = null;

            if (request.Origin != null || request.Destination != null || (request.ViaPoints != null && request.ViaPoints.First().Latitude != 0))
            {
                var origin = new GeoPoint(request.Origin.Latitude, request.Origin.Longitude);
                var destinations = new GeoPoint(request.Destination.Latitude, request.Destination.Longitude);

                if (request.ViaPoints != null && request.ViaPoints.First().Latitude == 0)
                    request.ViaPoints = null;

                result = await truckerPathApi.PlanRouteAsync(origin, destinations, request.ViaPoints, cancellationToken: cancellationToken);

                if (result == null)
                    return Result.Fail("ivalid route");
            }
            else
            {
                return Result.Fail("Origin and Destination cannot be null or empty.");
            }

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

            var fuelRoute = FuelRoute.CreateNew(
                request.TruckId, 
                new List<FuelRouteStation>(),
                new List<MapPoint>(),
                request.Weight);


            var originLocation = LocationPoint.CreateNew(request.OriginName, request.Origin.Latitude, request.Origin.Longitude, LocationPointType.Origin);
            var destinationLocation = LocationPoint.CreateNew(request.DestinationName, request.Destination.Latitude, request.Destination.Longitude, LocationPointType.Destination);

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

            routeSections.ForEach(rs =>
            {
                rs.SetOriginLocation(originLocation);
                rs.SetDestinationLocation(destinationLocation); 
            });


            await fuelRouteContext.LocationPoints.AddRangeAsync([originLocation, destinationLocation]);

            fuelRoute.SetRouteSections(routeSections);

            await fuelRouteContext.FuelRoutes.AddAsync(fuelRoute); 

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