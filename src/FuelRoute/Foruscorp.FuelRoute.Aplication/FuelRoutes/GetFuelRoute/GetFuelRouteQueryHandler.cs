using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Linq;
using static Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute.CreateFuelRouteCommandHandler;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute
{
    public record GetFuelRouteQuery(Guid RouteId) : IRequest<GetFuelRouteDto>;

    public class GetFuelRouteQueryHandler(
        IFuelRouteContext fuelRouteContext) : IRequestHandler<GetFuelRouteQuery, GetFuelRouteDto>
    {
        public async Task<GetFuelRouteDto> Handle(GetFuelRouteQuery request, CancellationToken cancellationToken)
        {
            var fuelRoad = await fuelRouteContext.FuelRoutes
                  .Include(x => x.OriginLocation)
                  .Include(x => x.DestinationLocation)
                  .Include(x => x.FuelRouteStations)
                  .Include(x => x.RouteSections /*.Where(x => x.Id == request.RouteSectionId)*/)
                  .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            //var fuelRoad = await fuelRouteContext.FuelRoutes
            //    .Include(x => x.FuelRouteStations.Where(st => st.RoadSectionId == request.RouteSectionId))
            //    .Include(x => x.RouteSections.FirstOrDefault(rs => rs.IsAssigned == true) /*.Where(x => x.Id == request.RouteSectionId)*/)
            //    .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if (fuelRoad == null)
                return new GetFuelRouteDto();

            var sectionId = fuelRoad.RouteSections.FirstOrDefault(rs => rs.IsAssigned == true);

            if (sectionId == null)
                sectionId = fuelRoad.RouteSections.First();

            var stations = fuelRoad.FuelRouteStations.Where(x => x.RoadSectionId == sectionId.Id).Select(fs => MapToDto(fs)).ToList();
            var routes = fuelRoad.RouteSections.Where(rs => rs.Id == sectionId.Id).Select(rs => new RouteDto
            {
                RouteSectionId = rs.Id.ToString(),
                MapPoints = PolylineEncoder.DecodePolyline(rs.EncodeRoute),
                RouteInfo = new RouteInfo(
                    rs.RouteSectionInfo.Tolls,
                    rs.RouteSectionInfo.Gallons,
                    rs.RouteSectionInfo.Miles,
                    rs.RouteSectionInfo.DriveTime)
            }).ToList();

            var fuelRoute = new GetFuelRouteDto
            {
               OriginName = fuelRoad.OriginLocation.Name,
               DestinationName = fuelRoad.DestinationLocation.Name,
               RouteId = fuelRoad.Id.ToString(),
               FuelStationDtos = stations,
               MapPoints = routes.SelectMany(r => r.MapPoints).ToList(),
            };

            return fuelRoute;
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

    public class GetFuelRouteDto
    {
        public string RouteId { get; set; }

        public string OriginName { get; set; } = "OriginName";  

        public string DestinationName { get; set; } = "DestinationName";

        public RouteInfo RouteInfo { get; set; }

        public List<List<double>> MapPoints { get; set; } = new List<List<double>>();

        public List<FuelStationDto> FuelStationDtos { get; set; } = new List<FuelStationDto>();
    }

    //public class GertRouteDto
    //{
    //    public string RouteSectionId { get; set; }

    //    public List<List<double>> MapPoints { get; set; } = new List<List<double>>();

    //    public RouteInfo RouteInfo { get; set; }
    //}
}
