using MediatR;
using System.Text.Json.Serialization;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute
{
    public class CreateFuelRouteCommand : IRequest<FuelRouteDto>
    {
        public GeoPoint Origin { get; set; }
        public GeoPoint Destination { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<GeoPoint> ViaPoints { get; set; } 

    }
}
