using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Foruscorp.FuelStations.Domain.FuelStations;
using Foruscorp.FuelStations.Aplication.Contructs;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class GetFuelStationsByRoadsQueryHandler(
        IMemoryCache memoryCache,
        IFuelStationContext fuelStationContext)
        : IRequestHandler<GetFuelStationsByRoadsQuery, Result<List<FuelStationDto>>>
    {
        private const double SearchRadiusKm = 20.0;
        private const double LatLonBuffer = 0.15;
        private const double TruckFuelConsumptionLPerKm = 0.3; // 30L/100km, adjustable
        private const double TruckTankCapacityL = 600.0; // Tank capacity in liters
        private const double MinFuelThresholdL = 50.0; // Minimum fuel to keep in tank



        public async Task<Result<List<FuelStationDto>>> Handlev1(GetFuelStationsByRoadsQuery request, CancellationToken cancellationToken)
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
                .DistinctBy(s => s.Id)
                .ToList();





            var stationsDto = uniqueStations
                .Select((station, index) => FuelStationToDto(station, index + 1)).ToList();

            //memoryCache.Set(cacheKey, stationsDto, TimeSpan.FromMinutes(30));

            return Result.Ok(stationsDto);
        }

    



        public async Task<Result<List<FuelStationDto>>> Handle(GetFuelStationsByRoadsQuery request, CancellationToken cancellationToken)
        {
            if (request?.Roads == null || !request.Roads.Any(r => r.Points?.Any(p => p?.Count >= 2) == true))
                return Result.Fail("No valid roads or points provided.");

            var points = request.Roads.SelectMany(r => r.Points.Where(p => p?.Count >= 2));
            var routePoints = points.Select(p => new GeoPoint(p[0], p[1])).ToList();

            var minLat = routePoints.Min(p => p.Latitude) - LatLonBuffer;
            var maxLat = routePoints.Max(p => p.Latitude) + LatLonBuffer;
            var minLon = routePoints.Min(p => p.Longitude) - LatLonBuffer;
            var maxLon = routePoints.Max(p => p.Longitude) + LatLonBuffer;

            var stations = await fuelStationContext.FuelStations
                .Include(s => s.FuelPrices)
                .AsNoTracking()
                .Where(s => s.Coordinates.Latitude >= minLat && s.Coordinates.Latitude <= maxLat &&
                            s.Coordinates.Longitude >= minLon && s.Coordinates.Longitude <= maxLon)
                .ToListAsync(cancellationToken);

            if (!stations.Any())
                return Result.Ok(new List<FuelStationDto>());

            var stopPlan = PlanStops(
                routePoints,
                stations,
                TruckFuelConsumptionLPerKm,
                60,
                TruckTankCapacityL
            );

            var stationsDto = stopPlan
                .Select((s, index) => FuelStationToDto(s.Station, index + 1))
                .ToList();

            return Result.Ok(stationsDto);
        }


        private List<FuelStopPlan> PlanStops(
    List<GeoPoint> routePoints,
    List<FuelStation> stations,
    double fuelConsumptionPer100Km,
    double currentFuelLiters,
    double tankCapacity)
        {
            var result = new List<FuelStopPlan>();
            double remainingFuel = currentFuelLiters;
            double distanceTraveled = 0;
            const double maxSearchRadius = 20.0;

            for (int i = 1; i < routePoints.Count; i++)
            {
                var segmentDistance = GeoCalculator.CalculateHaversineDistance(routePoints[i - 1], routePoints[i]);
                distanceTraveled += segmentDistance;
                double fuelNeeded = segmentDistance * fuelConsumptionPer100Km / 100;

                if (fuelNeeded > remainingFuel)
                {
                    var nearbyStations = stations
                        .Where(s => GeoCalculator.IsPointWithinRadius(routePoints[i - 1], s.Coordinates, maxSearchRadius))
                        .OrderBy(s => s.FuelPrices.First().Price) // найнижча ціна
                        .ToList();

                    if (!nearbyStations.Any())
                        throw new Exception("No fuel station found within reachable radius.");

                    var bestStation = nearbyStations.First();
                    double refillAmount = tankCapacity - remainingFuel;

                    result.Add(new FuelStopPlan
                    {
                        Station = bestStation,
                        RefillLiters = refillAmount,
                        StopAt = routePoints[i - 1]
                    });

                    remainingFuel = refillAmount;
                }

                remainingFuel -= fuelNeeded;
            }

            return result;
        }





        private List<FuelStationDto> FindOptimalFuelStops(
       List<FuelStationDto> stationsDto,
       List<GeoPoint> routePoints,
       double initialFuelLiters)
        {
            var optimalStops = new List<FuelStationDto>();
            var availableStations = new List<FuelStationDto>(stationsDto);
            double currentFuel = initialFuelLiters;
            double totalDistance = CalculateRouteDistance(routePoints);
            double currentDistance = 0;
            int stopOrder = 1;

            while (currentDistance < totalDistance && currentFuel > MinFuelThresholdL)
            {
                var maxReachableDistance = (currentFuel - MinFuelThresholdL) / TruckFuelConsumptionLPerKm;
                var remainingDistance = totalDistance - currentDistance;

                // Check if we can reach the destination without refueling
                if (maxReachableDistance >= remainingDistance)
                    break;

                // Find stations within reachable distance
                var reachable = availableStations
                    .Select(s =>
                    {
                        var stationPoint = new GeoPoint(
                            double.Parse(s.Latitude, System.Globalization.CultureInfo.InvariantCulture),
                            double.Parse(s.Longitude, System.Globalization.CultureInfo.InvariantCulture)
                        );

                        // Find the closest point on the route and its cumulative distance
                        var closestPointInfo = routePoints
                            .Select((p, i) => new
                            {
                                Point = p,
                                Index = i,
                                DistanceToStation = GeoCalculator.CalculateHaversineDistance(p, stationPoint),
                                CumulativeDistance = i == 0 ? 0 : CalculateRouteDistance(routePoints.Take(i + 1).ToList())
                            })
                            .OrderBy(p => p.DistanceToStation)
                            .First();

                        // Only consider stations ahead of current position
                        if (closestPointInfo.CumulativeDistance < currentDistance)
                            return null;

                        // Total distance to station: distance along route + perpendicular distance
                        var distanceAlongRoute = closestPointInfo.CumulativeDistance - currentDistance;
                        var totalDistanceToStation = distanceAlongRoute + closestPointInfo.DistanceToStation;

                        return new
                        {
                            Station = s,
                            TotalDistanceToStation = totalDistanceToStation,
                            DistanceToRoute = closestPointInfo.DistanceToStation,
                            CumulativeDistance = closestPointInfo.CumulativeDistance,
                            EffectivePrice = s.PriceAfterDiscount != null
                                ? double.Parse(s.PriceAfterDiscount, System.Globalization.CultureInfo.InvariantCulture)
                                : double.Parse(s.Price, System.Globalization.CultureInfo.InvariantCulture)
                        };
                    })
                    .Where(s => s != null && s.TotalDistanceToStation <= maxReachableDistance)
                    .OrderBy(s => s.EffectivePrice)
                    .ThenBy(s => s.TotalDistanceToStation)
                    .ToList();

                if (!reachable.Any())
                {
                    // If no station is reachable, try to find the closest one ahead
                    var closest = availableStations
                        .Select(s =>
                        {
                            var stationPoint = new GeoPoint(
                                double.Parse(s.Latitude, System.Globalization.CultureInfo.InvariantCulture),
                                double.Parse(s.Longitude, System.Globalization.CultureInfo.InvariantCulture)
                            );

                            var closestPointInfo = routePoints
                                .Select((p, i) => new
                                {
                                    Point = p,
                                    Index = i,
                                    DistanceToStation = GeoCalculator.CalculateHaversineDistance(p, stationPoint),
                                    CumulativeDistance = i == 0 ? 0 : CalculateRouteDistance(routePoints.Take(i + 1).ToList())
                                })
                                .OrderBy(p => p.DistanceToStation)
                                .First();

                            if (closestPointInfo.CumulativeDistance < currentDistance)
                                return null;

                            var distanceAlongRoute = closestPointInfo.CumulativeDistance - currentDistance;
                            var totalDistanceToStation = distanceAlongRoute + closestPointInfo.DistanceToStation;

                            return new
                            {
                                Station = s,
                                TotalDistanceToStation = totalDistanceToStation,
                                CumulativeDistance = closestPointInfo.CumulativeDistance
                            };
                        })
                        .Where(s => s != null)
                        .OrderBy(s => s.TotalDistanceToStation)
                        .FirstOrDefault();

                    if (closest != null && !optimalStops.Any(os => os.Id == closest.Station.Id))
                    {
                        var stop = closest.Station;
                        stop.StopOrder = stopOrder++;
                        optimalStops.Add(stop);
                        availableStations.Remove(stop);
                        currentFuel = TruckTankCapacityL;
                        currentDistance = closest.CumulativeDistance;
                    }
                    else
                    {
                        break; // Cannot continue without fuel
                    }
                    continue;
                }

                // Select the cheapest station within reach
                var bestStation = reachable.First().Station;
                bestStation.StopOrder = stopOrder++;
                optimalStops.Add(bestStation);
                availableStations.Remove(bestStation);

                // Calculate fuel needed to reach this station
                double distanceToStation = reachable.First().TotalDistanceToStation;
                double fuelConsumed = distanceToStation * TruckFuelConsumptionLPerKm;
                currentFuel -= fuelConsumed;
                currentDistance = reachable.First().CumulativeDistance;

                // Refuel to full tank
                currentFuel = TruckTankCapacityL;
            }

            return optimalStops;
        }

        private double CalculateRouteDistance(List<GeoPoint> points)
        {
            double totalDistance = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                totalDistance += GeoCalculator.CalculateHaversineDistance(points[i], points[i + 1]);
            }
            return totalDistance;
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
                Id = fuelStation.Id,
                Address = fuelStation.Address,
                Name = fuelStation.ProviderName,
                Latitude = fuelStation.Coordinates.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Longitude = fuelStation.Coordinates.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Price = fuelStation.FuelPrices.Any() ? fuelStation.FuelPrices.First().Price.ToString(System.Globalization.CultureInfo.InvariantCulture) : "0.00",
                Discount = fuelStation.FuelPrices.Any() && fuelStation.FuelPrices.First().DiscountedPrice.HasValue
                    ? fuelStation.FuelPrices.First().DiscountedPrice.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : null,
                PriceAfterDiscount = fuelStation.FuelPrices.Any() && fuelStation.FuelPrices.First().DiscountedPrice.HasValue
                    ? fuelStation.FuelPrices.First().PriceAfterDiscount.ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : null,
                StopOrder = index
            };
        }
    }


    public class FuelStopPlan
    {
        public FuelStation Station { get; set; }
        public GeoPoint StopAt { get; set; }
        public double RefillLiters { get; set; }
    }
}













//using MediatR;
//using FluentResults;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;
//using Foruscorp.FuelStations.Domain.FuelStations;
//using Foruscorp.FuelStations.Aplication.Contructs;

//namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
//{
//    public class GetFuelStationsByRoadsQueryHandler(
//        IMemoryCache memoryCache,
//        IFuelStationContext fuelStationContext)
//        : IRequestHandler<GetFuelStationsByRoadsQuery, Result<List<FuelStationDto>>>
//    {
//        private const double SearchRadiusKm = 20.0;
//        private const double LatLonBuffer = 0.15;
//        private const double TruckFuelConsumptionLPerKm = 0.3; // 30L/100km, adjustable
//        private const double TruckTankCapacityL = 600.0; // Tank capacity in liters
//        private const double MinFuelThresholdL = 50.0; // Minimum fuel to keep in tank

//        private record StationDistance(FuelStation Station, double DistanceToRoute, double EffectivePrice);

//        public async Task<Result<List<FuelStationDto>>> Handle(GetFuelStationsByRoadsQuery request, CancellationToken cancellationToken)
//        {
//            if (request?.Roads == null || !request.Roads.Any(r => r.Points?.Any(p => p?.Count >= 2) == true))
//                return Result.Fail("No valid roads or points provided.");

//            if (request.InitialFuelLiters <= 0 || request.InitialFuelLiters > TruckTankCapacityL)
//                return Result.Fail("Invalid initial fuel amount.");

//            var points = request.Roads.SelectMany(r => r.Points.Where(p => p?.Count >= 2));
//            var minLat = points.Min(p => p[0]) - LatLonBuffer;
//            var maxLat = points.Max(p => p[0]) + LatLonBuffer;
//            var minLon = points.Min(p => p[1]) - LatLonBuffer;
//            var maxLon = points.Max(p => p[1]) + LatLonBuffer;

//            var stations = await fuelStationContext.FuelStations
//                .AsNoTracking()
//                .Where(s => s.Coordinates.Latitude >= minLat && s.Coordinates.Latitude <= maxLat &&
//                            s.Coordinates.Longitude >= minLon && s.Coordinates.Longitude <= maxLon)
//                .ToListAsync(cancellationToken);

//            if (!stations.Any())
//                return Result.Ok(new List<FuelStationDto>());

//            var routePoints = request.Roads
//                .SelectMany(r => r.Points?.Where(p => p?.Count >= 2) ?? Enumerable.Empty<List<double>>())
//                .Select(p => new GeoPoint(p[0], p[1]))
//                .ToList();

//            var reachableStations = stations
//                .Select(s => new StationDistance(
//                    Station: s,
//                    DistanceToRoute: routePoints.Min(p => GeoCalculator.CalculateHaversineDistance(p, s.Coordinates)),
//                    EffectivePrice: s.FuelPrices.Any()
//                        ? (s.FuelPrices.First().DiscountedPrice ?? s.FuelPrices.First().Price)
//                        : double.MaxValue
//                ))
//                .Where(s => s.DistanceToRoute <= SearchRadiusKm)
//                .OrderBy(s => s.DistanceToRoute)
//                .ToList();

//            var optimalStops = CalculateOptimalStops(
//                reachableStations,
//                routePoints,
//                request.InitialFuelLiters
//            );

//            var stationsDto = optimalStops
//                .Select((station, index) => FuelStationToDto(station, index + 1))
//                .ToList();

//            return Result.Ok(stationsDto);
//        }

//        private List<FuelStation> CalculateOptimalStops(
//            List<StationDistance> reachableStations,
//            List<GeoPoint> routePoints,
//            double initialFuelLiters)
//        {
//            var optimalStops = new List<FuelStation>();
//            double currentFuel = initialFuelLiters;
//            double totalDistance = CalculateRouteDistance(routePoints);
//            double currentDistance = 0;
//            int currentPointIndex = 0;

//            while (currentPointIndex < routePoints.Count - 1 && currentFuel > MinFuelThresholdL)
//            {
//                var remainingDistance = totalDistance - currentDistance;
//                var maxReachableDistance = currentFuel / TruckFuelConsumptionLPerKm;

//                // Find stations within reachable distance
//                var reachable = reachableStations
//                    .Where(s => s.DistanceToRoute + currentDistance <= maxReachableDistance)
//                    .OrderBy(s => s.EffectivePrice)
//                    .ToList();

//                if (!reachable.Any())
//                {
//                    // If no station is reachable, try to find the closest one
//                    var closest = reachableStations
//                        .OrderBy(s => s.DistanceToRoute)
//                        .FirstOrDefault();
//                    if (closest != null && !optimalStops.Contains(closest.Station))
//                    {
//                        optimalStops.Add(closest.Station);
//                        currentFuel = TruckTankCapacityL; // Assume full tank after refueling
//                    }
//                    break;
//                }

//                // Select the cheapest station within reach
//                var bestStation = reachable.First().Station;
//                optimalStops.Add(bestStation);

//                // Calculate fuel needed to reach this station
//                double distanceToStation = reachable.First().DistanceToRoute;
//                double fuelConsumed = distanceToStation * TruckFuelConsumptionLPerKm;
//                currentFuel -= fuelConsumed;
//                currentDistance += distanceToStation;

//                // Refuel to full tank
//                currentFuel = TruckTankCapacityL;

//                // Move to next route segment
//                currentPointIndex++;
//            }

//            return optimalStops;
//        }

//        private double CalculateRouteDistance(List<GeoPoint> points)
//        {
//            double totalDistance = 0;
//            for (int i = 0; i < points.Count - 1; i++)
//            {
//                totalDistance += GeoCalculator.CalculateHaversineDistance(points[i], points[i + 1]);
//            }
//            return totalDistance;
//        }

//        private static string GenerateCacheKey(List<Road> roads)
//        {
//            var roadIds = string.Join("_", roads.Select(r => r.Id).OrderBy(id => id));
//            var pointsHash = roads
//                .SelectMany(r => r.Points?.Select(p => $"{p[0]:F2}_{p[1]:F2}") ?? Enumerable.Empty<string>())
//                .OrderBy(p => p)
//                .Aggregate("", (current, p) => current + p);
//            return $"FuelStations_{roadIds}_{pointsHash.GetHashCode()}";
//        }

//        private FuelStationDto FuelStationToDto(FuelStation fuelStation, int index = 0)
//        {
//            return new FuelStationDto
//            {
//                Id = fuelStation.Id,
//                Address = fuelStation.Address,
//                Name = fuelStation.ProviderName,
//                Latitude = fuelStation.Coordinates.Latitude.ToString(),
//                Longitude = fuelStation.Coordinates.Longitude.ToString(),
//                Price = fuelStation.FuelPrices.Any() ? fuelStation.FuelPrices.First().Price.ToString() : "0.00",
//                Discount = fuelStation.FuelPrices.Any() && fuelStation.FuelPrices.First().DiscountedPrice.HasValue
//                    ? fuelStation.FuelPrices.First().DiscountedPrice.Value.ToString()
//                    : null,
//                PriceAfterDiscount = fuelStation.FuelPrices.Any() && fuelStation.FuelPrices.First().DiscountedPrice.HasValue
//                    ? fuelStation.FuelPrices.First().PriceAfterDiscount.ToString()
//                    : null,
//                StopOrder = index
//            };
//        }
//    }
//}