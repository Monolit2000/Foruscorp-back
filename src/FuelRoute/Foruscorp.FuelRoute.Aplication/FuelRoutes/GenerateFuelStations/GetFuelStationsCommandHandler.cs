using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using FuelStationDto = Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.FuelStationDto;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GenerateFuelStations
{
    public class GetFuelStationsCommandHandler(
        ISender sender,
        IFuelRouteContext fuelRouteContext) : IRequestHandler<GetFuelStationsCommand, Result<List<FuelStationDto>>>
    {
        public async Task<Result<List<FuelStationDto>>> Handle(GetFuelStationsCommand request, CancellationToken cancellationToken)
        {
            var fuelRoad = await fuelRouteContext.FuelRoutes
                .Include(x => x.FuelRouteStations)
                .Include(x => x.RouteSections)
                .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if(fuelRoad == null)
                return Result.Fail($"Fuel route with id:{request.RouteId} not found.");    



            var routeSections = fuelRoad.RouteSections
                .Where(x => request.RouteSectionIds.Count > 0 && request.RouteSectionIds.Contains(x.Id.ToString()))
                .ToList();

            if (!routeSections.Any())
                routeSections = fuelRoad.RouteSections;

            if (!routeSections.Any())
                return Result.Fail("No route sections found for the provided IDs.");    

            var roadSectionDtos = new List<RoadSectionDto>();
            foreach (var routeSection in routeSections)
            {
                var polylinePoints = PolylineEncoder.DecodePolyline(routeSection.EncodeRoute);   

                var roadSectionDto = new RoadSectionDto
                {
                    RoadSectionId = routeSection.Id.ToString(),
                    Points = polylinePoints
                };

                roadSectionDtos.Add(roadSectionDto);
            }

            var fuelStationsResult = await sender.Send(new GetFuelStationsByRoadsQuery {Roads = roadSectionDtos, RequiredFuelStations = request.RequiredFuelStations}, cancellationToken);

            if (fuelStationsResult.IsFailed)
                return Result.Fail(fuelStationsResult.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve fuel stations.");

            var fuelStations = fuelStationsResult.Value.Select(x => MapToFuelStation(x, fuelRoad.Id));


            var oldStations = await fuelRouteContext.FuelRouteStation
                 .Where(x => x.FuelRouteId == fuelRoad.Id)
                 .ToListAsync(cancellationToken);

            fuelRouteContext.FuelRouteStation.RemoveRange(oldStations);


            await fuelRouteContext.FuelRouteStation.AddRangeAsync(fuelStations);
            await fuelRouteContext.SaveChangesAsync(cancellationToken);


            return fuelStationsResult.Value;
        }


        public static FuelRouteStation MapToFuelStation(FuelStationDto dto, Guid fuelRouteId)
        {
            return new FuelRouteStation
            {
                FuelRouteId = fuelRouteId,
                //FuelPointId = dto.Id,

                //Price = decimal.TryParse(dto.Price, out var price) ? price : 0m,
                //Discount = decimal.TryParse(dto.Discount, out var discount) ? discount : 0m,
                //PriceAfterDiscount = decimal.TryParse(dto.PriceAfterDiscount, out var afterDiscount) ? afterDiscount : 0m,

                //Latitude = dto.Latitude,
                //Longitude = dto.Longitude,

                //Name = dto.Name,
                //Address = dto.Address,

                IsAlgorithm = dto.IsAlgorithm,
                Refill = dto.Refill,
                StopOrder = dto.StopOrder,
                //NextDistanceKm = dto.NextDistanceKm,
                //RoadSectionId = dto.RoadSectionId
            };
        }
    }
}
