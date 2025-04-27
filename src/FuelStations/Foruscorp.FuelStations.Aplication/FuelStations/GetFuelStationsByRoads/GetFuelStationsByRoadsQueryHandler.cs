
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
        IFuelStationContext fuelStationContext,
        IFuelStationsService fuelStationsService)
        : IRequestHandler<GetFuelStationsByRoadsQuery, Result<List<FuelStationDto>>>
    {
        private const double SearchRadiusKm = 15.0;
        private const double LatLonBuffer = 0.15; // Примерно 15 км в градусах (грубая оценка)

        public async Task<Result<List<FuelStationDto>>> Handle(GetFuelStationsByRoadsQuery request, CancellationToken cancellationToken)
        {
            if (request?.Roads == null || !request.Roads.Any(r => r.Points?.Any(p => p?.Count >= 2) == true))
            {
                return Result.Fail("No valid roads or points provided.");
            }

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
            {
                return Result.Ok(new List<FuelStationDto>());
            }

            // Проверяем кэш
            string cacheKey = GenerateCacheKey(request.Roads);
            if (memoryCache.TryGetValue(cacheKey, out List<FuelStationDto> cachedStations))
            {
                return Result.Ok(cachedStations);
            }

            var uniqueStations = request.Roads
                .SelectMany(r => r.Points?.Where(p => p?.Count >= 2) ?? Enumerable.Empty<List<double>>())
                .Select(p => new GeoPoint(p[0], p[1]))
                .SelectMany(geoPoint => stations
                    .Where(s => GeoCalculator.IsPointWithinRadius(geoPoint, s.Coordinates, SearchRadiusKm)))
                .Distinct() 
                .ToList();

            var stationsDto = uniqueStations
                .Select((station, index) => new FuelStationDto
                {
                    Id = index + 1,
                    Address = station.Address,
                    Latitude = station.Coordinates.Latitude.ToString("F6"),
                    Longitude = station.Coordinates.Longitude.ToString("F6"),
                    Price = station.FuelPrices.Any() ? station.FuelPrices.First().Price.ToString("F2") : "0.00",
                    Discount = station.FuelPrices.Any() && station.FuelPrices.First().DiscountedPrice.HasValue
                        ? station.FuelPrices.First().DiscountedPrice.Value.ToString("F2")
                        : null
                })
                .ToList();

            memoryCache.Set(cacheKey, stationsDto, TimeSpan.FromMinutes(30));

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
    }
}
































//using FluentResults;
//using Foruscorp.FuelStations.Aplication.Contructs;
//using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
//using Foruscorp.FuelStations.Domain.FuelStations;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
//{
//    public class GetFuelStationsByRoadsQueryHandler(
//        IMemoryCache memoryCache,
//        IFuelStationContext fuelStationContext,
//        IFuelStationsService fuelStationsService)
//        : IRequestHandler<GetFuelStationsByRoadsQuery, Result<List<FuelStationDto>>>
//    {
//        private const double SearchRadiusKm = 15.0; 

//        public async Task<Result<List<FuelStationDto>>> Handle(GetFuelStationsByRoadsQuery request, CancellationToken cancellationToken)
//        {
//            if (request == null || request.Roads == null || !request.Roads.Any() || request.Roads.All(r => r.Points == null || !r.Points.Any()))
//            {
//                return Result.Fail("No valid roads or points provided.");
//            }

//            var stations = await fuelStationContext.FuelStations
//                .AsNoTracking()
//                .ToListAsync(cancellationToken);

//            if (!stations.Any())
//            {
//                return Result.Ok(new List<FuelStationDto>()); 
//            }

//            var uniqueStations = new HashSet<FuelStation>();

//            foreach (var road in request.Roads)
//            {
//                if (road.Points == null || !road.Points.Any())
//                    continue;

//                foreach (var point in road.Points)
//                {
//                    if (point == null || point.Count < 2)
//                        continue;

//                    var geoPoint = new GeoPoint(point[0], point[1]); // [lat, lng]

//                    var nearbyStations = stations
//                        .Where(station => GeoCalculator.IsPointWithinRadius(
//                            center: geoPoint,
//                            point: station.Coordinates,
//                            radiusKm: SearchRadiusKm))
//                        .ToList();

//                    foreach (var station in nearbyStations)
//                    {
//                        uniqueStations.Add(station);
//                    }
//                }
//            }

//            var stationsDto = uniqueStations.Select((station, index) => new FuelStationDto
//            {
//                Id = index + 1,
//                Address = station.Address,
//                Latitude = station.Coordinates.Latitude.ToString(),
//                Longitude = station.Coordinates.Longitude.ToString(),
//                Price = station.FuelPrices.First().Price.ToString(),
//                Discount = station.FuelPrices.First().DiscountedPrice?.ToString(),
//            }).ToList();

//            return Result.Ok(stationsDto);
//        }

//        public FuelStationDto ToFuelStationDto(FuelStationResponce fuelStationResponce)
//        {
//            return new FuelStationDto
//            {
//                Id = fuelStationResponce.Id,
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