using MediatR;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using Microsoft.Extensions.Caching.Memory;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRadius
{
    public class GetFuelStationsByRadiusQueryHandler(
        IMemoryCache memoryCache,
        IFuelStationContext fuelStationContext,
        IFuelStationsService fuelStationsService) : IRequestHandler<GetFuelStationsByRadiusQuery, IEnumerable<FuelStationDto>>
    {

        //public async Task<IEnumerable<FuelStationDto>> Handle(GetFuelStationsByRadiusQuery request, CancellationToken cancellationToken)
        //{
        //    var cacheKey = $"FuelStations_{request.Radius}_{request.source}_{request.destination}";
        //    var cacheDateKey = $"{cacheKey}_Date";

        //    if (memoryCache.TryGetValue(cacheKey, out IEnumerable<FuelStationDto> cachedStations) &&
        //        memoryCache.TryGetValue(cacheDateKey, out DateTime cachedDate) &&
        //        cachedDate.Date == DateTime.Today)
        //    {
        //        return cachedStations;
        //    }

        //    var stations = await fuelStationsService.GetFuelStations(
        //        "14200|gfwKLLaTKkeJ6spYXXLk2X7q2eoMhjt6xflo3Zyo",
        //        request.Radius,
        //        request.source,
        //        request.destination);

        //    var fuelStationDtos = stations.Select(stationResponse => new FuelStationDto
        //    {
        //        Id = stationResponse.Id,
        //        Latitude = stationResponse.Latitude,
        //        Longitude = stationResponse.Longitude,
        //        Name = stationResponse.Name,
        //        Address = stationResponse.Address,
        //        State = stationResponse.State,
        //        Price = stationResponse.Price,
        //        Discount = stationResponse.Discount,
        //        PriceAfterDiscount = stationResponse.PriceAfterDiscount,
        //        DistanceToLocation = stationResponse.DistanceToLocation,
        //        Route = stationResponse.Route
        //    }).ToList();

        //    var cacheEntryOptions = new MemoryCacheEntryOptions
        //    {
        //        AbsoluteExpiration = DateTime.Today.AddDays(1).AddTicks(-1),
        //        Priority = CacheItemPriority.Normal
        //    };

        //    memoryCache.Set(cacheKey, fuelStationDtos, cacheEntryOptions);
        //    memoryCache.Set(cacheDateKey, DateTime.Today, cacheEntryOptions);

        //    return fuelStationDtos;
        //}


        public async Task<IEnumerable<FuelStationDto>> Handle(GetFuelStationsByRadiusQuery request, CancellationToken cancellationToken)
        {

            var stations = await fuelStationsService.GetFuelStations(
                "14200|gfwKLLaTKkeJ6spYXXLk2X7q2eoMhjt6xflo3Zyo",
                request.Radius,
                request.source,
                request.destination);

            return stations.Select(stationResponce => new FuelStationDto
            {
                Id = stationResponce.Id,
                Latitude = stationResponce.Latitude,
                Longitude = stationResponce.Longitude,
                Name = stationResponce.Name,
                Address = stationResponce.Address,
                State = stationResponce.State,
                Price = stationResponce.GetPriceAsString(),
                Discount = stationResponce.GetDiscountAsStringl(),
                PriceAfterDiscount = stationResponce.PriceAfterDiscount,
                DistanceToLocation = stationResponce.GetDistanceToLocationAsString(),
                Route = stationResponce.Route
            });
        }
    }
}
