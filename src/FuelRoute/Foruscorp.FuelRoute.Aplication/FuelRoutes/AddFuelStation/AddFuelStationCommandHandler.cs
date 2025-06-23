using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.GetFuelStationsByRoadsQueryHandler;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AddFuelStation
{
    public class AddFuelStationCommandHandler(
        ISender sender,
        IFuelRouteContext fuelRouteContext) : IRequestHandler<AddFuelStationCommand, Result<List<FuelStationDto>>>
    {
        public async Task<Result<List<FuelStationDto>>> Handle(AddFuelStationCommand request, CancellationToken cancellationToken)
        {
            var fuelRoad = await fuelRouteContext.FuelRoutes
               .Include(x => x.FuelRouteStations)
               .Include(x => x.RouteSections)
               .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);


            var fuelRoadStations = fuelRoad.FuelRouteStations
                .Where(x => x.IsAlgorithm)
                .Select(s => new RequiredStationDto
                {
                    StationId = s.FuelStationId,
                    RefillLiters = double.TryParse(s.Refill, out var refill) ? refill : 0.0
                })
                .ToList();

            if(!request.RequiredFuelStations.Any())
                return Result.Fail("Required fuel stations list cannot be empty.");

            var requestStationsDict = request.RequiredFuelStations
                .ToDictionary(x => x.StationId, x => x);

            var mergedStations = fuelRoadStations
                .Select(s => requestStationsDict.TryGetValue(s.StationId, out var reqStation) ? reqStation : s)
                .ToList();

            var extraRequestStations = request.RequiredFuelStations
                .Where(x => !fuelRoadStations.Any(y => y.StationId == x.StationId));

            mergedStations.AddRange(extraRequestStations);

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

            var fuelStationsResult = await sender.Send(new GetFuelStationsByRoadsQuery { Roads = roadSectionDtos, RequiredFuelStations = mergedStations}, cancellationToken);

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
                FuelPointId = dto.Id,

                Price = decimal.TryParse(dto.Price, out var price) ? price : 0m,
                Discount = decimal.TryParse(dto.Discount, out var discount) ? discount : 0m,
                PriceAfterDiscount = decimal.TryParse(dto.PriceAfterDiscount, out var afterDiscount) ? afterDiscount : 0m,

                Latitude = dto.Latitude,
                Longitude = dto.Longitude,

                Name = dto.Name,
                Address = dto.Address,

                IsAlgorithm = dto.IsAlgorithm,
                Refill = dto.Refill,
                StopOrder = dto.StopOrder,
                NextDistanceKm = dto.NextDistanceKm,
                RoadSectionId = Guid.Parse(dto.RoadSectionId)
            };
        }
    }
}
