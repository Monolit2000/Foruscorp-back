using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute.CreateFuelRouteCommandHandler;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetAasignedRouteByTruckId
{

    public class AssignedWithPassedRouteByTruckIdDto
    {
        public CurrentLocation CurrentLocation { get; set; }
        public AssignedRoute AssignedRoute { get; set; }
        public TrackedRouteDto PassedRoute { get; set; }
    }

    public class CurrentLocation
    {
        public string FormattedLocation { get; set; }
        public GeoPoint Location { get; set; }
    }


    public class AssignedRoute 
    {
        public string RouteId { get; set; }
        public string OriginName { get; set; }
        public string DestinationName { get; set; }
        public GeoPoint Origin { get; set; }
        public GeoPoint Destination { get; set; }
        public double Weight { get; set; }
        public double RemainingFuel { get; set; }
        public string SectionId { get; set; }
        public List<List<double>> MapPoints { get; set; } = new List<List<double>>();
        public List<FuelStationDto> FuelStationDtos { get; set; } = new List<FuelStationDto>();
        public RouteInfo RouteInfo { get; set; }
    }


    public class GetAasignedRouteByTruckIdResponce
    {
        public bool HasAssigned { get; set; } = false;

        public AssignedWithPassedRouteByTruckIdDto AssignedWithPassedRoute { get; set; }
    
    }
    public record GetAasignedRouteByTruckIdQuery(Guid TruckId) : IRequest<Result<GetAasignedRouteByTruckIdResponce>>;
    public class GetAasignedRouteByTruckIdQueryHandler(
        IFuelRouteContext fuelRouteContext,
        ITruckTrackingService truckClient) : IRequestHandler<GetAasignedRouteByTruckIdQuery, Result<GetAasignedRouteByTruckIdResponce>>
    {
        public async Task<Result<GetAasignedRouteByTruckIdResponce>> Handle(GetAasignedRouteByTruckIdQuery request, CancellationToken cancellationToken)
        {
            var passedRoute = await truckClient.GetRouteAsync(request.TruckId);

            if (passedRoute.CurrentLocation == null)
                return Result.Fail("Route not found for the given TruckId.");

            var currentLocation = new CurrentLocation
            {
                FormattedLocation = passedRoute.FormattedLocation,
                Location = new GeoPoint(passedRoute.CurrentLocation.Latitude, passedRoute.CurrentLocation.Longitude)
            };

            if (!passedRoute.RouteId.HasValue || !passedRoute.IsRoute)
            {
                return new GetAasignedRouteByTruckIdResponce
                {
                    HasAssigned = false,
                    AssignedWithPassedRoute = new AssignedWithPassedRouteByTruckIdDto
                    {
                        CurrentLocation = currentLocation,
                        AssignedRoute = null,
                        PassedRoute = null
                    }
                };
            }

            var fuelRoad = await fuelRouteContext.FuelRoutes
                  .Include(x => x.RouteSections.Where(x => x.IsAccepted == true))
                    .ThenInclude(x => x.LocationPoints)
                  .FirstOrDefaultAsync(x => x.Id == passedRoute.RouteId, cancellationToken);

            if (fuelRoad == null)
                return new GetAasignedRouteByTruckIdResponce
                {
                    HasAssigned = false,
                    AssignedWithPassedRoute = null
                };

            var section = fuelRoad.RouteSections.FirstOrDefault(rs => rs.IsAccepted == true);

            if (section == null)
                section = fuelRoad.RouteSections.First();

            var stationsContextes = await fuelRouteContext.FuelRouteStation.Where(x => x.RoadSectionId == section.Id).ToListAsync(); 
            var stations = stationsContextes.Select(fs => MapToDto(fs)).ToList();

            var routes = new RouteDto
            {
                RouteSectionId = section.Id.ToString(),
                MapPoints = PolylineEncoder.DecodePolyline(section.EncodeRoute),
                RouteInfo = new RouteInfo(
                    section.RouteSectionInfo.Tolls,
                    section.RouteSectionInfo.Gallons,
                    section.RouteSectionInfo.Miles,
                    section.RouteSectionInfo.DriveTime)
            };

            var orignPoint = section.GetOriginLocation();
            var destinationPoint = section.GetDestinationLocation();

            var assignedRoute = new AssignedRoute
            {
                RemainingFuel = fuelRoad.RemainingFuel,
                Weight = fuelRoad.Weight,
                OriginName = orignPoint.Name,
                DestinationName = destinationPoint.Name,
                Origin = new GeoPoint(orignPoint.Latitude, orignPoint.Longitude),
                Destination = new GeoPoint(destinationPoint.Latitude, destinationPoint.Longitude),
                RouteId = fuelRoad.Id.ToString(),
                FuelStationDtos = stations,
                SectionId = section.Id.ToString(),
                RouteInfo = routes.RouteInfo,
                MapPoints = routes.MapPoints.ToList(),
            };

            var assignedWithPassedRoute = new AssignedWithPassedRouteByTruckIdDto
            {
                CurrentLocation = currentLocation,
                AssignedRoute = assignedRoute,
                PassedRoute = passedRoute,
            };

            return new GetAasignedRouteByTruckIdResponce
            {
                HasAssigned = true,
                AssignedWithPassedRoute = assignedWithPassedRoute
            };
        }

        public static FuelStationDto MapToDto(FuelRouteStation station)
        {
            return new FuelStationDto
            {
                Id = station.FuelPointId,
                Name = station.Name,
                Address = station.Address,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                Price = station.Price.ToString(CultureInfo.InvariantCulture),
                Discount = station.Discount.ToString(CultureInfo.InvariantCulture),
                PriceAfterDiscount = station.PriceAfterDiscount.ToString(CultureInfo.InvariantCulture),
                IsAlgorithm = station.IsAlgorithm,
                Refill = station.Refill,
                StopOrder = station.StopOrder,
                NextDistanceKm = station.NextDistanceKm,
                RoadSectionId = station.RoadSectionId.ToString(),
                CurrentFuel = station.CurrentFuel
            };
        }
    }
}
