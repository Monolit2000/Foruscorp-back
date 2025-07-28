using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.GetFuelStationsByRoadsQueryHandler;
using FuelStationDto = Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads.FuelStationDto;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.PlanFuelStations
{
    public class PlanFuelStationsCommandHandler(
        ISender sender,
        IFuelRouteContext fuelRouteContext) : IRequestHandler<PlanFuelStationsCommand, Result<PlanFuelStationsByRoadsResponce>>
    {
        public async Task<Result<PlanFuelStationsByRoadsResponce>> Handle(PlanFuelStationsCommand request, CancellationToken cancellationToken)
        {
            var fuelRoad = await fuelRouteContext.FuelRoutes
                //.Include(x => x.FuelRouteStations.Where(frs => !frs.IsOld))
                .Include(x => x.RouteSections.Where(rs => request.RouteSectionIds.Count > 0 && request.RouteSectionIds.Contains(rs.Id.ToString())))
                .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);


            //var sectionGuids = request.RouteSectionIds
            //    .Where(s => !string.IsNullOrWhiteSpace(s))
            //    .Select(Guid.Parse)
            //    .ToList();

            //IQueryable<FuelRoute> query = fuelRouteContext.FuelRoutes
            //    .Include(r => r.FuelRouteStations.Where(st => st.IsOld));

            //if (sectionGuids.Any())
            //{
            //    // Only add the filtered Include if there are section IDs
            //    query = query.Include(r =>
            //        r.RouteSections.Where(sec => sectionGuids.Contains(sec.Id))
            //    );
            //}

            //var fuelRoad = await query
            //    .FirstOrDefaultAsync(r => r.Id == request.RouteId, cancellationToken);

            if (fuelRoad == null)
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

            fuelRoad.Weight = request.Weight;   

            var requiredStationDtos = new List<RequiredStationDto>(request.RequiredFuelStations);


            var fuelStationsResult = await sender.Send(new GetFuelStationsByRoadsQuery {Weight = request.Weight, Roads = roadSectionDtos, RequiredFuelStations = requiredStationDtos, FinishFuel = request.FinishFuel, FuelProviderNameList = request.FuelProviderNameList, CurrentFuel = request.CurrentFuel }, cancellationToken);

            if (fuelStationsResult.IsFailed)
                return Result.Fail(fuelStationsResult.Errors.FirstOrDefault()?.Message ?? "Failed to retrieve fuel stations.");

            var fuelStations = fuelStationsResult.Value.FuelStations.Select(x => MapToFuelStation(x, fuelRoad.Id)).ToList();


            var sectionIds = roadSectionDtos
                .Select(dto => Guid.Parse(dto.RoadSectionId))
                .ToHashSet();

            var oldStations = await fuelRouteContext.FuelRouteStation
                .Where(x => x.FuelRouteId == fuelRoad.Id && !x.IsOld)
                .ToListAsync(cancellationToken);

            //Mark old stations as old
            foreach (var station in oldStations)
                station.MurkAsOld();

            Console.WriteLine(  );
            fuelRouteContext.FuelRouteStation.UpdateRange(oldStations);

            //Add new stations with route version
            foreach (var station in fuelStations)
                station.RouteVersion = fuelRoad.RouteVersion;

            await fuelRouteContext.FuelRouteStation.AddRangeAsync(fuelStations);

            var refillPerSection = fuelStations
                .Where(x => x.IsAlgorithm)
                .Select(x => new {
                    x.RoadSectionId,
                    Refill = decimal.TryParse(x.Refill, out var refill) ? refill : (decimal?)null
                })
                .Where(x => x.Refill.HasValue)
                .GroupBy(x => x.RoadSectionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Refill.Value)
                );

            foreach (var section in routeSections)
            {
                if (refillPerSection.TryGetValue(section.Id, out var totalRefill))
                {
                    section.FuelNeeded = totalRefill;
                }
                else
                {
                    section.FuelNeeded = 0.0m;
                }
            }


            fuelRoad.RemainingFuel = fuelStationsResult.Value.FinishInfo.RemainingFuelLiters;


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
                RoadSectionId = Guid.Parse(dto.RoadSectionId),

                CurrentFuel = dto.CurrentFuel
            };
        }
    }
}
