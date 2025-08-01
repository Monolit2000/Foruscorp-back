﻿using Foruscorp.FuelRoutes.Aplication.Contruct;
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
                  .Include(x => x.FuelRouteStations.Where(frs => !frs.IsOld))
                  .Include(x => x.RouteSections.Where(x => x.IsAssigned == true))
                  .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if (fuelRoad == null)
                return new GetFuelRouteDto();

            var section = fuelRoad.RouteSections.FirstOrDefault(rs => rs.IsAssigned == true);

            if (section == null)
                section = fuelRoad.RouteSections.First();

            var stations = fuelRoad.FuelRouteStations.Where(x => x.RoadSectionId == section.Id).Select(fs => MapToDto(fs)).ToList();
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

            var fuelRoute = new GetFuelRouteDto
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

        public GeoPoint Origin { get; set; }
        public GeoPoint Destination { get; set; }

        public double Weight { get; set; } = 0.0;   

        public RouteInfo RouteInfo { get; set; }
        
        public double RemainingFuel { get; set; }
        public string SectionId { get; set; }
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
