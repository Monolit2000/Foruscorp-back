using MediatR;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute
{
    public record GetFuelRouteQuery(Guid RouteId, Guid RouteSectionId) : IRequest<FuelRouteDto>;

    public class GetFuelRouteQueryHandler(
        IFuelRouteContext fuelRouteContext) : IRequestHandler<GetFuelRouteQuery, FuelRouteDto>
    {
        public async Task<FuelRouteDto> Handle(GetFuelRouteQuery request, CancellationToken cancellationToken)
        {
            var fuelRoad = await fuelRouteContext.FuelRoutes
                .Include(x => x.FuelRouteStations.Where(st => st.RoadSectionId == request.RouteSectionId))
                .Include(x => x.RouteSections.Where(x => x.Id == request.RouteSectionId))
                .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if (fuelRoad == null)
                return new FuelRouteDto();

            var stations = fuelRoad.FuelRouteStations.Select(fs => MapToDto(fs)).ToList();
            var routes = fuelRoad.RouteSections.Select(rs => new RouteDto
            {
                RouteSectionId = rs.Id.ToString(),
                MapPoints = PolylineEncoder.DecodePolyline(rs.EncodeRoute)
            }).ToList();

            var fuelRoute = new FuelRouteDto
            {
               RouteId = fuelRoad.Id.ToString(),
               FuelStationDtos = stations,
               RouteDtos = routes
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
}
