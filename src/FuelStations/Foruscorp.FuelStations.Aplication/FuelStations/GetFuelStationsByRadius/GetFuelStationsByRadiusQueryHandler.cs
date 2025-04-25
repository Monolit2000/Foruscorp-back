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

        //    // Check if cache exists and is still valid (less than 6 hours old)
        //    if (memoryCache.TryGetValue(cacheKey, out IEnumerable<FuelStationDto> cachedStations) &&
        //        memoryCache.TryGetValue(cacheDateKey, out DateTime cachedDate) &&
        //        DateTime.Now < cachedDate.AddHours(6))
        //    {
        //        return cachedStations;
        //    }

        //    var stations = await fuelStationsService.GetFuelStations(
        //        "14200|gfwKLLaTKkeJ6spYXXLk2X7q2eoMhjt6xflo3Zyo",
        //        request.Radius,
        //        request.source,
        //        request.destination);

        //    var fuelStationDtos = stations.Select(stationResponce 
        //        => ToFuelStationDto(stationResponce)).ToList();

        //    var cacheEntryOptions = new MemoryCacheEntryOptions
        //    {
        //        AbsoluteExpiration = DateTimeOffset.Now.AddHours(6), // Expire after 6 hours from now
        //        Priority = CacheItemPriority.Normal
        //    };

        //    memoryCache.Set(cacheKey, fuelStationDtos, cacheEntryOptions);
        //    memoryCache.Set(cacheDateKey, DateTime.Today, cacheEntryOptions);

        //    return  fuelStationDtos;
        //}


        public async Task<IEnumerable<FuelStationDto>> Handle(GetFuelStationsByRadiusQuery request, CancellationToken cancellationToken)
        {

            var stations = await fuelStationsService.GetFuelStations(
                "14200|gfwKLLaTKkeJ6spYXXLk2X7q2eoMhjt6xflo3Zyo",
                request.Radius,
                request.source,
                request.destination);

            if (!stations.Any() || stations.FirstOrDefault() == null)
                return Enumerable.Empty<FuelStationDto>();

            return stations.Select(stationResponce => ToFuelStationDto(stationResponce));         
        }    
        
        
        public FuelStationDto ToFuelStationDto(FuelStationResponce fuelStationResponce)
        {
            return new FuelStationDto
            {
                Id = fuelStationResponce.Id,
                Latitude = fuelStationResponce.Latitude,
                Longitude = fuelStationResponce.Longitude,
                Name = fuelStationResponce.Name,
                Address = fuelStationResponce.Address,
                State = fuelStationResponce.State,
                Price = fuelStationResponce.GetPriceAsString(),
                Discount = fuelStationResponce.GetDiscountAsStringl(),
                PriceAfterDiscount = fuelStationResponce.PriceAfterDiscount,
                DistanceToLocation = fuelStationResponce.GetDistanceToLocationAsString(),
                Route = fuelStationResponce.Route
            };  
        }
    }
}
