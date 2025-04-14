using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Foruscorp.FuelRoutes.Aplication.Contruct.Route
{
    public class RouteResponce
    {



    }

    public class RouteRequest
    {
        public string Start { get; set; }
        public string End { get; set; }
        public string FuelType { get; set; }
        public int MaxDistance { get; set; }
        public int MaxTime { get; set; }
        public int MaxCost { get; set; }
        public bool IsReturnTrip { get; set; }
        public bool IsFastestRoute { get; set; }
        public bool IsCheapestRoute { get; set; }
    }

    public class Route
    {
        public string Start { get; set; }
        public string End { get; set; }
        public string FuelType { get; set; }
        public int MaxDistance { get; set; }
        public int MaxTime { get; set; }
        public int MaxCost { get; set; }
        public bool IsReturnTrip { get; set; }
        public bool IsFastestRoute { get; set; }
        public bool IsCheapestRoute { get; set; }

        public List<RoutePoint> RoutePoints { get; set; } = [];
    }

    public class RoutePoint
    {
        public decimal Latitude { get; private set; }
        public decimal Longitude { get; private set; }
    }



  // Top-level response
        public class ApiResponse
        {
            [JsonPropertyName("code")]
            public int Code { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("data")]
            public string Data { get; set; } // This will be deserialized into DataObject

            [JsonPropertyName("api_errors")]
            public object ApiErrors { get; set; } // Can be null or an object/array
        }

        // Structure for the deserialized 'data' field
        public class DataObject
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("routeVersion")]
            public int RouteVersion { get; set; }

            [JsonPropertyName("routes")]
            public Routes Routes { get; set; }
        }

        public class Routes
        {
            [JsonPropertyName("waypointsAndShapes")]
            public List<WaypointOrRouteSection> WaypointsAndShapes { get; set; }

            [JsonPropertyName("concatenated_routes")]
            public List<ConcatenatedRoute> ConcatenatedRoutes { get; set; }
        }

        // Represents either a waypoint or a route section in waypointsAndShapes array
        public class WaypointOrRouteSection
        {
            [JsonPropertyName("objectType")]
            public string ObjectType { get; set; } // "waypoint" or "routeSection"

            // Waypoint-specific properties
            [JsonPropertyName("originalLocation")]
            public Location OriginalLocation { get; set; }

            [JsonPropertyName("location")]
            public Location Location { get; set; }

            [JsonPropertyName("waypointType")]
            public string WaypointType { get; set; } // e.g., "departure", "arrival"

            [JsonPropertyName("placeType")]
            public string PlaceType { get; set; }

            [JsonPropertyName("dwellTime")]
            public int DwellTime { get; set; }

            [JsonPropertyName("index")]
            public int? Index { get; set; }

            [JsonPropertyName("departureTime")]
            public string DepartureTime { get; set; }

            [JsonPropertyName("addressName")]
            public string AddressName { get; set; }

            // RouteSection-specific properties
            [JsonPropertyName("sections")]
            public List<RouteSection> Sections { get; set; }
        }

        public class Location
        {
            [JsonPropertyName("lng")]
            public double Lng { get; set; }

            [JsonPropertyName("lat")]
            public double Lat { get; set; }
        }

        public class RouteSection
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("showShape")]
            public List<List<double>> ShowShape { get; set; } // List of [lat, lng]

            [JsonPropertyName("summary")]
            public Summary Summary { get; set; }

            [JsonPropertyName("maneuver")]
            public List<Maneuver> Maneuver { get; set; }

            [JsonPropertyName("strategy")]
            public Dictionary<string, object> Strategy { get; set; }

            [JsonPropertyName("tolls")]
            public List<object> Tolls { get; set; }

            [JsonPropertyName("notices")]
            public List<object> Notices { get; set; }

            [JsonPropertyName("routeHandle")]
            public string RouteHandle { get; set; }

            [JsonPropertyName("violation")]
            public List<object> Violation { get; set; }
        }

        public class Summary
        {
            [JsonPropertyName("duration")]
            public int Duration { get; set; }

            [JsonPropertyName("length")]
            public int Length { get; set; }
        }

        public class Maneuver
        {
            [JsonPropertyName("travelTime")]
            public int TravelTime { get; set; }

            [JsonPropertyName("offset")]
            public int Offset { get; set; }

            [JsonPropertyName("instruction")]
            public string Instruction { get; set; }

            [JsonPropertyName("length")]
            public double Length { get; set; }

            [JsonPropertyName("action")]
            public string Action { get; set; }

            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("position")]
            public Position Position { get; set; }

            [JsonPropertyName("direction")]
            public string Direction { get; set; }
        }

        public class Position
        {
            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }
        }

        public class ConcatenatedRoute
        {
            [JsonPropertyName("ids")]
            public List<string> Ids { get; set; }

            [JsonPropertyName("length")]
            public int Length { get; set; }

            [JsonPropertyName("duration")]
            public int Duration { get; set; }

            [JsonPropertyName("routeHandle")]
            public string RouteHandle { get; set; }

            [JsonPropertyName("section_tolls")]
            public List<object> SectionTolls { get; set; }

            [JsonPropertyName("tolls")]
            public double Tolls { get; set; } // Changed from int to double
        }
}








