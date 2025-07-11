﻿using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute
{
    public class GetPussedFuelRouteQueryHandler(
        IFuelRouteContext fuelRouteContext,
        ITruckTrackingService truckClient) : IRequestHandler<GetPussedFuelRouteQuery, Result<RoutInfoDto>>
    {
        public async Task<Result<RoutInfoDto>> Handle(GetPussedFuelRouteQuery request, CancellationToken cancellationToken)
        {
            var route = await truckClient.GetRouteAsync(request.TruckId);

            var fuelRoute = await fuelRouteContext.FuelRoutes
                .Include(x => x.OriginLocation)
                .Include(x => x.DestinationLocation)
                .FirstOrDefaultAsync(x => x.Id == route.RouteId);
            if (fuelRoute == null)
            {

                return Result.Ok(new RoutInfoDto
                {
                    TruckId = request.TruckId,
                    //OriginName = "Unknown",
                    //DestinationName = "Unknown",
                    //Origin = new GeoPoint(0, 0),
                    //Destination = new GeoPoint(0, 0),
                    routeDto = route,
                    Weight = 0
                });
            }

            var fuelRouteDto = new RoutInfoDto
            {
                TruckId = fuelRoute.TruckId,
                OriginName = fuelRoute.OriginLocation.Name,
                DestinationName = fuelRoute.DestinationLocation.Name,
                Origin = new GeoPoint(fuelRoute.OriginLocation.Latitude, fuelRoute.OriginLocation.Longitude),
                Destination = new GeoPoint(fuelRoute.DestinationLocation.Latitude, fuelRoute.DestinationLocation.Longitude),
                routeDto = route,
                Weight = fuelRoute.Weight   
            };

            return Result.Ok(fuelRouteDto);
        }
    }

    public class RoutInfoDto
    {
        public Guid TruckId { get; set; }
        public string OriginName { get; set; }
        public string DestinationName { get; set; }
        public GeoPoint Origin { get; set; }
        public GeoPoint Destination { get; set; }
        public TrackedRouteDto routeDto { get; set; }
        public double Weight { get; set; }  
    }
}
