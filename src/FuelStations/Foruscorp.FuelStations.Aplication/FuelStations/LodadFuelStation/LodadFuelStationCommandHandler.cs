using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using Foruscorp.FuelStations.Domain.FuelStations;
using System.Globalization;

namespace Foruscorp.FuelStations.Aplication.FuelStations.LodadFuelStation
{
    public class LodadFuelStationCommandHandler(
        IMemoryCache memoryCache,
        IFuelStationContext fuelStationContext,
        IFuelStationsService fuelStationsService) : IRequestHandler<LodadFuelStationCommand>
    {
        public async Task Handle(LodadFuelStationCommand request, CancellationToken cancellationToken)
        {
            var stationResponce = await fuelStationsService.GetFuelStations(
                "14200|gfwKLLaTKkeJ6spYXXLk2X7q2eoMhjt6xflo3Zyo",
                2000);

            if (!stationResponce.Any() || stationResponce.FirstOrDefault() == null)
                return;

            var stations = stationResponce.Select(stationResponce => FuelStation.CreateNew(
                stationResponce.Address,
                new GeoPoint(
                    double.Parse(stationResponce.Latitude, System.Globalization.CultureInfo.InvariantCulture),
                    double.Parse(stationResponce.Longitude, System.Globalization.CultureInfo.InvariantCulture)
                ),
                [
                    new FuelPrice(
                        FuelType.Gasoline95,
                        SafeParseDouble(stationResponce.GetPriceAsString()),
                        SafeParseDouble(stationResponce.GetDiscountAsStringl()))
                ]
            )).ToList();

            await fuelStationContext.FuelStations.AddRangeAsync(stations, cancellationToken);

            await fuelStationContext.SaveChangesAsync(cancellationToken);

            var stationDtos = stationResponce.Select(sr => ToFuelStationDto(sr));



        }

        public static double SafeParseDouble(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return double.NaN; 

            return double.TryParse(
                value,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out var result)
                ? result
                : double.NaN; 
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
