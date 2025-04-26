using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.FuelStations
{
    public static class Mapping
    {
        public static FuelStationDto ToFuelStationDto(FuelStationResponce fuelStationResponce)
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
