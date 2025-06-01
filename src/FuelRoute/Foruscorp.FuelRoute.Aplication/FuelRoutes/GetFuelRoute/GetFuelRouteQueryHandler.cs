using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute
{
    public class GetFuelRouteQueryHandler(
        IFuelRouteContext fuelRouteContext) : IRequestHandler<GetFuelRouteQuery, Result<FuelRouteDto>>
    {
        public async Task<Result<FuelRouteDto>> Handle(GetFuelRouteQuery request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.RequestRouteId))
                return Result.Fail("Id is null");

            var fuelRoute = await fuelRouteContext.FuelRoutes.FirstOrDefaultAsync(x => x.Id == request.RouteId);
            if (fuelRoute == null)
                return Result.Fail("Route not found");

            var decodedRoute = PolylineEncoder.DecodePolyline(fuelRoute.EncodeRoute);

            var routeDto = new RouteDto
            {
                RouteId = "1",
                MapPoints = decodedRoute
            };

            //return new FuelRouteDto
            //{
            //    ResponseId = result.Id,
            //    RouteDtos = new List<RouteDto>(){routeDto},
            //    FuelStationDtos = fuelStations
            //};

            throw new NotImplementedException();
        }
    }
}
