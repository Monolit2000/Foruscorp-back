using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Domain.RouteValidators;
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
                  //.Include(x => x.FuelRouteStations.Where(frs => !frs.IsOld))
                  .Include(x => x.RouteSections.Where(x => x.IsAssigned == true))
                    .ThenInclude(re => re.LocationPoints)
                  .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

            if (fuelRoad == null)
                return new GetFuelRouteDto();

            var section = fuelRoad.RouteSections
                .Where(rs => rs.IsAssigned)
                .OrderByDescending(rs => rs.AssignedAt)
                .FirstOrDefault();

            if (section == null)
                section = await fuelRouteContext.RouteSections
                    .Where(x => x.RouteId == fuelRoad.Id && x.IsAccepted)
                    .Include(re => re.LocationPoints)
                    .FirstOrDefaultAsync();

            var routeValidator = await fuelRouteContext.RouteValidators
                    .Include(rv => rv.FuelStationChanges)
                        .ThenInclude(fsc => fsc.FuelStation)
                    .FirstOrDefaultAsync(rv => rv.FuelRouteSectionId == section.Id, cancellationToken);

            // Используем FuelStationChange из RouteValidator, если он существует
            var stations = new List<FuelStationDto>();
            if (routeValidator != null && routeValidator?.FuelStationChanges != null && routeValidator.FuelStationChanges.Any())
            {
                // Получаем все FuelStationChange из RouteValidator
                var stationChanges = routeValidator.FuelStationChanges
                    .OrderBy(fsc => fsc.StopOrder)
                    .Select(fsc => MapToDto(fsc))
                    .ToList();

                // Получаем все FuelRouteStation для этого section
                var fuelRouteStations = await fuelRouteContext.FuelRouteStation
                    .Where(x => x.RoadSectionId == section.Id)
                    .ToListAsync();

                // Создаем словарь для быстрого поиска по Id
                var stationChangesDict = stationChanges.ToDictionary(s => s.Id);

                // Добавляем все FuelStationChange
                stations.AddRange(stationChanges);

                // Добавляем только те FuelRouteStation, которые:
                // 1. Не найдены в FuelStationChange (по Id)
                // 2. Имеют IsAlgorithm == false (не алгоритмические)
                var additionalStations = fuelRouteStations
                    .Where(frs => !stationChangesDict.ContainsKey(frs.FuelPointId) && !frs.IsAlgorithm)
                    .Select(frs => MapToDto(frs))
                    .ToList();

                stations.AddRange(additionalStations);
            }
            else
            {
                // Fallback к старым FuelRouteStation, если RouteValidator не найден
                stations = await fuelRouteContext.FuelRouteStation
                    .Where(x => x.RoadSectionId == section.Id)
                    .Select(fs => MapToDto(fs))
                    .ToListAsync();
            }

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

            var orignPoint = section.LocationPoints.FirstOrDefault(lp => lp.Type == LocationPointType.Origin);
            var destinationPoint = section.LocationPoints.FirstOrDefault(lp => lp.Type == LocationPointType.Destination);

            var fuelRoute = new GetFuelRouteDto
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
                IsValid = routeValidator?.IsValid,

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
                Refill = station.Refill.ToString(CultureInfo.InvariantCulture),
                StopOrder = station.StopOrder,
                NextDistanceKm = station.NextDistanceKm.ToString(CultureInfo.InvariantCulture),

                RoadSectionId = station.RoadSectionId.ToString(),

                CurrentFuel = station.CurrentFuel,

                IsManual = false, 
                ForwardDistants = station.ForwardDistance,
            };
        }

        public static FuelStationDto MapToDto(FuelStationChange station)
        {
            return new FuelStationDto
            {
                Id = station.FuelStation.FuelPointId,
                Name = station.FuelStation.Name,
                Address = station.FuelStation.Address,
                Latitude = station.FuelStation.Latitude,
                Longitude = station.FuelStation.Longitude,
                Price = station.FuelStation.Price.ToString(CultureInfo.InvariantCulture),
                Discount = station.FuelStation.Discount.ToString(CultureInfo.InvariantCulture),
                PriceAfterDiscount = station.FuelStation.PriceAfterDiscount.ToString(CultureInfo.InvariantCulture),

                IsAlgorithm = station.FuelStation.IsAlgorithm,
                Refill = station.Refill.ToString(CultureInfo.InvariantCulture),
                StopOrder = station.StopOrder,
                NextDistanceKm = station.NextDistanceKm.ToString(CultureInfo.InvariantCulture),

                RoadSectionId = station.FuelStation.RoadSectionId.ToString(),

                CurrentFuel = station.CurrentFuel,

                IsManual = station.IsManual,
                ForwardDistants = station.ForwardDistance,
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
        public string? RouteValidatorId { get; set; }

        public bool? IsValid { get; set; } = true;    
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
