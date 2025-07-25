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
        public AssignedRoute AssignedRoute { get; set; }
        public TrackedRouteDto routeDto { get; set; }
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

            if (passedRoute == null)
                return Result.Fail("Route not found for the given TruckId.");

            if (!passedRoute.RouteId.HasValue || !passedRoute.IsRoute)
            {
                return new GetAasignedRouteByTruckIdResponce
                {
                    HasAssigned = false,
                    AssignedWithPassedRoute = null
                };
            }

            var fuelRoad = await fuelRouteContext.FuelRoutes
                  .Include(x => x.OriginLocation)
                  .Include(x => x.DestinationLocation)
                  //.Include(x => x.FuelRouteStations.Where(frs => !frs.IsOld))
                  .Include(x => x.RouteSections.Where(x => x.IsAssigned == true))
                  .FirstOrDefaultAsync(x => x.Id == passedRoute.RouteId, cancellationToken);

            if (fuelRoad == null)
                return new GetAasignedRouteByTruckIdResponce
                {
                    HasAssigned = false,
                    AssignedWithPassedRoute = null
                };

            var section = fuelRoad.RouteSections.FirstOrDefault(rs => rs.IsAssigned == true);

            if (section == null)
                section = fuelRoad.RouteSections.First();

            var stationsContextes = await fuelRouteContext.FuelRouteStation.Where(x => x.RoadSectionId == section.Id).ToListAsync(); 
            var stations = stationsContextes.Select(fs => MapToDto(fs)).ToList();

            var routes = fuelRoad.RouteSections.Where(rs => rs.Id == section.Id).Select(rs => new RouteDto
            {
                RouteSectionId = section.Id.ToString(),
                MapPoints = PolylineEncoder.DecodePolyline(rs.EncodeRoute),
                RouteInfo = new RouteInfo(
                    rs.RouteSectionInfo.Tolls,
                    rs.RouteSectionInfo.Gallons,
                    rs.RouteSectionInfo.Miles,
                    rs.RouteSectionInfo.DriveTime)
            }).ToList();

            var assignedRoute = new AssignedRoute
            {
                RemainingFuel = fuelRoad.RemainingFuel,
                Weight = fuelRoad.Weight,
                OriginName = fuelRoad.OriginLocation.Name,
                DestinationName = fuelRoad.DestinationLocation.Name,
                Origin = new GeoPoint(fuelRoad.OriginLocation.Latitude, fuelRoad.OriginLocation.Longitude),
                Destination = new GeoPoint(fuelRoad.DestinationLocation.Latitude, fuelRoad.DestinationLocation.Longitude),
                RouteId = fuelRoad.Id.ToString(),
                FuelStationDtos = stations,
                SectionId = section.Id.ToString(),
                RouteInfo = routes.FirstOrDefault().RouteInfo,
                MapPoints = routes.SelectMany(r => r.MapPoints).ToList(),
            };

            var assignedWithPassedRoute = new AssignedWithPassedRouteByTruckIdDto
            {
                AssignedRoute = assignedRoute,
                routeDto = passedRoute,
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
