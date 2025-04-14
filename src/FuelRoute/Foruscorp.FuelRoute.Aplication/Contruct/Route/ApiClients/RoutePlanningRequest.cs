using System.Text.Json.Serialization;

namespace Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients
{
    public class RoutePlanningRequest
    {
        [JsonPropertyName("needWholePoint")]
        public bool NeedWholePoint { get; set; }

        [JsonPropertyName("routeDescription")]
        public string RouteDescription { get; set; }

        [JsonPropertyName("isNavigation")]
        public int IsNavigation { get; set; }

        [JsonPropertyName("wayPoints")]
        public WayPoint[] WayPoints { get; set; }

        [JsonPropertyName("routeOption")]
        public RouteOption RouteOption { get; set; }

        [JsonPropertyName("affected")]
        public Affected Affected { get; set; }
    }

    public class WayPoint
    {
        [JsonPropertyName("originalPosition")]
        public TPosition OriginalPosition { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("placeType")]
        public string PlaceType { get; set; }

        [JsonPropertyName("addressName")]
        public string AddressName { get; set; }

        [JsonPropertyName("dwellTime")]
        public int DwellTime { get; set; }
    }

    public class TPosition
    {
        [JsonPropertyName("course")]
        public int Course { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    public class RouteOption
    {
        [JsonPropertyName("departureTime")]
        public string DepartureTime { get; set; }

        [JsonPropertyName("routingMode")]
        public string RoutingMode { get; set; }

        [JsonPropertyName("resultLimit")]
        public int ResultLimit { get; set; }

        [JsonPropertyName("routingType")]
        public string RoutingType { get; set; }

        [JsonPropertyName("routingOption")]
        public RoutingOption RoutingOption { get; set; }

        [JsonPropertyName("speedProfile")]
        public string SpeedProfile { get; set; }

        [JsonPropertyName("truckModel")]
        public TruckModel TruckModel { get; set; }
    }

    public class RoutingOption
    {
        [JsonPropertyName("dirtRoad")]
        public bool DirtRoad { get; set; }

        [JsonPropertyName("toll")]
        public bool Toll { get; set; }

        [JsonPropertyName("tunnel")]
        public bool Tunnel { get; set; }

        [JsonPropertyName("uTurns")]
        public bool UTurns { get; set; }

        [JsonPropertyName("ferry")]
        public bool Ferry { get; set; }
    }

    public class TruckModel
    {
        [JsonPropertyName("limitedVehicleWeight")]
        public double LimitedVehicleWeight { get; set; }

        [JsonPropertyName("weightPerAxle")]
        public double WeightPerAxle { get; set; }

        [JsonPropertyName("vehicleHeight")]
        public double VehicleHeight { get; set; }

        [JsonPropertyName("vehicleWidth")]
        public double VehicleWidth { get; set; }

        [JsonPropertyName("vehicleLength")]
        public double VehicleLength { get; set; }

        [JsonPropertyName("truckAxleCount")]
        public int TruckAxleCount { get; set; }

        [JsonPropertyName("trailersCount")]
        public int TrailersCount { get; set; }

        [JsonPropertyName("hazardousGoods")]
        public string HazardousGoods { get; set; }
    }

    public class Affected
    {
        [JsonPropertyName("linkid")]
        public string[] LinkId { get; set; }

        [JsonPropertyName("area")]
        public string[] Area { get; set; }

        [JsonPropertyName("polygon")]
        public string[] Polygon { get; set; }
    }
}
