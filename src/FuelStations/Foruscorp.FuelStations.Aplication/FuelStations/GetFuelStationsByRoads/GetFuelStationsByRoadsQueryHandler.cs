using FluentResults;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using Foruscorp.FuelStations.Domain.FuelStations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads
{
    public class GetFuelStationsByRoadsQueryHandler(
        IMemoryCache memoryCache,
        IFuelStationContext fuelStationContext,
        IFuelStationsService fuelStationsService)
        : IRequestHandler<GetFuelStationsByRoadsQuery, Result<List<FuelStationDto>>>
    {
        private const double SearchRadiusKm = 15.0; 

        public async Task<Result<List<FuelStationDto>>> Handle(GetFuelStationsByRoadsQuery request, CancellationToken cancellationToken)
        {
            if (request == null || request.Roads == null || !request.Roads.Any() || request.Roads.All(r => r.Points == null || !r.Points.Any()))
            {
                return Result.Fail("No valid roads or points provided.");
            }

            var stations = await fuelStationContext.FuelStations
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!stations.Any())
            {
                return Result.Ok(new List<FuelStationDto>()); 
            }

            var uniqueStations = new HashSet<FuelStation>();

            foreach (var road in request.Roads)
            {
                if (road.Points == null || !road.Points.Any())
                    continue;

                foreach (var point in road.Points)
                {
                    if (point == null || point.Count < 2)
                        continue;

                    var geoPoint = new GeoPoint(point[0], point[1]); // [lat, lng]

                    var nearbyStations = stations
                        .Where(station => GeoCalculator.IsPointWithinRadius(
                            center: geoPoint,
                            point: station.Coordinates,
                            radiusKm: SearchRadiusKm))
                        .ToList();

                    foreach (var station in nearbyStations)
                    {
                        uniqueStations.Add(station);
                    }
                }
            }

            var stationsDto = uniqueStations.Select(station => new FuelStationDto
            {
                Address = station.Address,
                Latitude = station.Coordinates.Latitude.ToString(),
                Longitude = station.Coordinates.Longitude.ToString(),
                Price = station.FuelPrices.First().Price.ToString(),
                Discount = station.FuelPrices.First().DiscountedPrice?.ToString(),
            }).ToList();

            return Result.Ok(stationsDto);
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