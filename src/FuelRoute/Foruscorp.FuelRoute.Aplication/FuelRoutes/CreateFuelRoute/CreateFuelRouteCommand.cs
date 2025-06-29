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
        public GeoPoint Origin { get; set; }
        public GeoPoint Destination { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GeoPoint> ViaPoints { get; set; } 

    }
}
