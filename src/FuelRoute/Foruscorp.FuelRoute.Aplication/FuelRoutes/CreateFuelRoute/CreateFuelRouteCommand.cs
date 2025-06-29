using MediatR;
using System.Text.Json.Serialization;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using FluentResults;
using Foruscorp.FuelStations.Aplication.FuelStations.GetFuelStationsByRoads;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute
{
    public class CreateFuelRouteCommand : IRequest<Result<FuelRouteDto>>
    {
        public Guid TruckId { get; set; }
        public string OriginName { get; set; } = "OriginName";
        public string DestinationName { get; set; } = "DestinationName";
        public GeoPoint Origin { get; set; }
        public GeoPoint Destination { get; set; }

        public double Weight { get; set; } = 40000.0; // in Paunds

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GeoPoint> ViaPoints { get; set; } 

    }
}
