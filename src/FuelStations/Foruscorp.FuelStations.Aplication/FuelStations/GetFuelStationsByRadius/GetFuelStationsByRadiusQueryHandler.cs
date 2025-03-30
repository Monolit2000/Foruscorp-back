using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using MediatR;

namespace Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRadius
{
    public class GetFuelStationsByRadiusQueryHandler(
        IFuelStationContext fuelStationContext,
        IFuelStationsService fuelStationsService) : IRequestHandler<GetFuelStationsByRadiusQuery, IEnumerable<FuelStationDto>>
    {
        public async Task<IEnumerable<FuelStationDto>> Handle(GetFuelStationsByRadiusQuery request, CancellationToken cancellationToken)
        {

            var stations = await fuelStationsService.GetFuelStations(
                "14200|gfwKLLaTKkeJ6spYXXLk2X7q2eoMhjt6xflo3Zyo",
                request.Radius);

            return stations.Select(stationResponce => new FuelStationDto 
            {
                Id = stationResponce.Id,
                Latitude = stationResponce.Latitude,
                Longitude = stationResponce.Longitude,
                Name = stationResponce.Name,
                Address = stationResponce.Address,
                State = stationResponce.State,
                Price = stationResponce.Price,
                Discount = stationResponce.Discount,
                PriceAfterDiscount = stationResponce.PriceAfterDiscount,
                DistanceToLocation = stationResponce.DistanceToLocation,
                Route = stationResponce.Route
            });


        }
    }
}
