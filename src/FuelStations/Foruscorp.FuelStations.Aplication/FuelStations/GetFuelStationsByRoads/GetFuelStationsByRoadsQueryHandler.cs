
using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;


namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class GetFuelStationsByRoadsQueryHandler(
        IMemoryCache memoryCache,
        IFuelStationContext fuelStationContext)
        : IRequestHandler<GetFuelStationsByRoadsQuery, Result<List<FuelStationDto>>>
    {
        private const double SearchRadiusKm = 15.0;
        private const double LatLonBuffer = 0.15;

        public async Task<Result<List<FuelStationDto>>> Handle(GetFuelStationsByRoadsQuery request, CancellationToken cancellationToken)
        {
            if (request?.Roads == null || !request.Roads.Any(r => r.Points?.Any(p => p?.Count >= 2) == true))
                return Result.Fail("No valid roads or points provided.");

            var points = request.Roads.SelectMany(r => r.Points.Where(p => p?.Count >= 2));
            var minLat = points.Min(p => p[0]) - LatLonBuffer;
            var maxLat = points.Max(p => p[0]) + LatLonBuffer;
            var minLon = points.Min(p => p[1]) - LatLonBuffer;
            var maxLon = points.Max(p => p[1]) + LatLonBuffer;

            var stations = await fuelStationContext.FuelStations
                .AsNoTracking()
                .Where(s => s.Coordinates.Latitude >= minLat && s.Coordinates.Latitude <= maxLat &&
                            s.Coordinates.Longitude >= minLon && s.Coordinates.Longitude <= maxLon)
                .ToListAsync(cancellationToken);


            if (!stations.Any())
                return Result.Ok(new List<FuelStationDto>());

            //string cacheKey = GenerateCacheKey(request.Roads);
            //if (memoryCache.TryGetValue(cacheKey, out List<FuelStationDto> cachedStations))
            //    return Result.Ok(cachedStations);


            var uniqueStations = request.Roads
                .SelectMany(r => r.Points?.Where(p => p?.Count >= 2) ?? Enumerable.Empty<List<double>>())
                .Select(p => new GeoPoint(p[0], p[1]))
                .SelectMany(geoPoint => stations
                    .Where(s => GeoCalculator.IsPointWithinRadius(geoPoint, s.Coordinates, SearchRadiusKm)))
                .Distinct()
                .ToList();

            var stationsDto = uniqueStations
                .Select((station, index) => FuelStationToDto(station, index + 1)).ToList();

            //memoryCache.Set(cacheKey, stationsDto, TimeSpan.FromMinutes(30));

            return Result.Ok(stationsDto);
        }

        private static string GenerateCacheKey(List<Road> roads)
        {
            var roadIds = string.Join("_", roads.Select(r => r.Id).OrderBy(id => id));
            var pointsHash = roads
                .SelectMany(r => r.Points?.Select(p => $"{p[0]:F2}_{p[1]:F2}") ?? Enumerable.Empty<string>())
                .OrderBy(p => p)
                .Aggregate("", (current, p) => current + p);
            return $"FuelStations_{roadIds}_{pointsHash.GetHashCode()}";
        }

        private FuelStationDto FuelStationToDto(FuelStation fuelStation, int index = 0)
        {
            return new FuelStationDto
            {
                Id = index,
                Address = fuelStation.Address,
                Name = fuelStation.ProviderName,    
                Latitude = fuelStation.Coordinates.Latitude.ToString(),
                Longitude = fuelStation.Coordinates.Longitude.ToString(),
                Price = fuelStation.FuelPrices.Any() ? fuelStation.FuelPrices.First().Price.ToString() : "0.00",
                Discount = fuelStation.FuelPrices.Any() && fuelStation.FuelPrices.First().DiscountedPrice.HasValue
                    ? fuelStation.FuelPrices.First().DiscountedPrice.Value.ToString()
                    : null,
                PriceAfterDiscount = fuelStation.FuelPrices.Any() && fuelStation.FuelPrices.First().DiscountedPrice.HasValue
                    ? fuelStation.FuelPrices.First().PriceAfterDiscount.ToString()
                    : null,
            };
        }
    }
}
