//using MediatR;
//using Foruscorp.FuelStations.Aplication.Contructs;
//using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
//using Microsoft.Extensions.Caching.Memory;

//namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRadius
//{
//    public class GetFuelStationsByRadiusQueryHandler(
//        IMemoryCache memoryCache,
//        IFuelStationContext fuelStationContext,
//        IFuelStationsService fuelStationsService) : IRequestHandler<GetFuelStationsByRadiusQuery, IEnumerable<FuelStationDto>>
//    {
//        public async Task<IEnumerable<FuelStationDto>> Handle(GetFuelStationsByRadiusQuery request, CancellationToken cancellationToken)
//        {

//            var stations = await fuelStationsService.GetFuelStations(
//                "14200|gfwKLLaTKkeJ6spYXXLk2X7q2eoMhjt6xflo3Zyo",
//                request.Radius,
//                request.source,
//                request.destination);

//            if (!stations.Any() || stations.FirstOrDefault() == null)
//                return Enumerable.Empty<FuelStationDto>();

//            return stations.Select(stationResponce => ToFuelStationDto(stationResponce));         
//        }    
        
        
//        public FuelStationDto ToFuelStationDto(FuelStationResponce fuelStationResponce)
//        {
//            return new FuelStationDto
//            {
//                RoadSectionId = fuelStationResponce.RoadSectionId,
//                Latitude = fuelStationResponce.Latitude,
//                Longitude = fuelStationResponce.Longitude,
//                Name = fuelStationResponce.Name,
//                Address = fuelStationResponce.Address,
//                State = fuelStationResponce.State,
//                Price = fuelStationResponce.GetPriceAsString(),
//                Discount = fuelStationResponce.GetDiscountAsStringl(),
//                PriceAfterDiscount = fuelStationResponce.PriceAfterDiscount,
//                DistanceToLocation = fuelStationResponce.GetDistanceToLocationAsString(),
//                Route = fuelStationResponce.Route
//            };  
//        }
//    }
//}
