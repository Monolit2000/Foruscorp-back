//using FluentResults;
//using Foruscorp.FuelRoutes.Aplication.Contruct;
//using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
//using Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute;
//using Foruscorp.FuelRoutes.Domain.FuelRoutes;
//using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using System.Globalization;
//using static Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute.CreateFuelRouteCommandHandler;

//namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRouteByVersion
//{
//    public record GetFuelRouteByVersionCommand(Guid RouteId, int Version) : IRequest<Result<FuelRouteDto>>;
//    public class GetFuelRouteByVersionCommandHandler(
//        IFuelRouteContext fuelRouteContext) : IRequestHandler<GetFuelRouteByVersionCommand, Result<FuelRouteDto>>
//    {
//        public async Task<Result<FuelRouteDto>> Handle(GetFuelRouteByVersionCommand request, CancellationToken cancellationToken)
//        {

//            var fuelRoad = await fuelRouteContext.FuelRoutes
//              .Include(x => x.FuelRouteStations.Where(frs => !frs.IsOld))
//              .Include(x => x.RouteSections /*.Where(x => x.Id == request.RouteSectionId)*/)
//              .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

//            //var fuelRoad = await fuelRouteContext.FuelRoutes
//            //    .Include(x => x.FuelRouteStations.Where(st => st.RoadSectionId == request.RouteSectionId))
//            //    .Include(x => x.RouteSections.FirstOrDefault(rs => rs.IsAssigned == true) /*.Where(x => x.Id == request.RouteSectionId)*/)
//            //    .FirstOrDefaultAsync(x => x.Id == request.RouteId, cancellationToken);

//            if (fuelRoad == null)
//                return new Result<FuelRouteDto>();

//            var section = fuelRoad.RouteSections.FirstOrDefault(rs => rs.IsAssigned == true);

//            if (section == null)
//                section = fuelRoad.RouteSections.First();

//            var stations = fuelRoad.FuelRouteStations.Where(x => x.RoadSectionId == section.Id).Select(fs => MapToDto(fs)).ToList();
//            var routes = fuelRoad.RouteSections.Where(rs => rs.Id == section.Id).Select(rs => new RouteDto
//            {
//                RouteSectionId = rs.Id.ToString(),
//                MapPoints = PolylineEncoder.DecodePolyline(rs.EncodeRoute),
//                RouteInfo = new RouteInfo(
//                    rs.RouteSectionInfo.Tolls,
//                    rs.RouteSectionInfo.Gallons,
//                    rs.RouteSectionInfo.Miles,
//                    rs.RouteSectionInfo.DriveTime)
//            }).ToList();

//            var fuelRoute = new GetFuelRouteDto
//            {
//                RemainingFuel = fuelRoad.RemainingFuel,
//                Weight = fuelRoad.Weight,
//                OriginName = fuelRoad.OriginLocation.Name,
//                DestinationName = fuelRoad.DestinationLocation.Name,
//                RouteId = fuelRoad.Id.ToString(),
//                FuelStationDtos = stations,
//                RouteInfo = routes.FirstOrDefault().RouteInfo,
//                MapPoints = routes.SelectMany(r => r.MapPoints).ToList(),
//            };


//            var responce  = new FuelRouteDto
//            {
             
//            };


//            return responce;

//        }

//        public static FuelStationDto MapToDto(FuelRouteStation station)
//        {
//            return new FuelStationDto
//            {
//                Id = station.FuelPointId,
//                Name = station.Name,
//                Address = station.Address,
//                Latitude = station.Latitude,
//                Longitude = station.Longitude,
//                Price = station.Price.ToString(CultureInfo.InvariantCulture),
//                Discount = station.Discount.ToString(CultureInfo.InvariantCulture),
//                PriceAfterDiscount = station.PriceAfterDiscount.ToString(CultureInfo.InvariantCulture),

//                IsAlgorithm = station.IsAlgorithm,
//                Refill = station.Refill,
//                StopOrder = station.StopOrder,
//                NextDistanceKm = station.NextDistanceKm,

//                RoadSectionId = station.RoadSectionId.ToString(),

//                CurrentFuel = station.CurrentFuel
//            };
//        }
//    }
//}
