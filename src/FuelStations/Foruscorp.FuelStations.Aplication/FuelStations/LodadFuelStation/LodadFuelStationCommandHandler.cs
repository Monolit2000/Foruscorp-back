using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;

namespace Foruscorp.FuelStations.Aplication.FuelStations.LodadFuelStation
{
    public class LodadFuelStationCommandHandler(
        IMemoryCache memoryCache,
        IFuelStationContext fuelStationContext,
        IFuelStationsService fuelStationsService) : IRequestHandler<LodadFuelStationCommand>
    {
        public async Task Handle(LodadFuelStationCommand request, CancellationToken cancellationToken)
        {
            var stations = await fuelStationsService.GetFuelStations(
                "14200|gfwKLLaTKkeJ6spYXXLk2X7q2eoMhjt6xflo3Zyo",
                2000);

            if (!stations.Any() || stations.FirstOrDefault() == null)
                return;

            var stationDtos = stations.Select(stationResponce => ToFuelStationDto(stationResponce));
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
