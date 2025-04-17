using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients
{
    // Основной класс ответа
    public class DropPointResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public DropPointData Data { get; set; }

        [JsonPropertyName("apiErrors")]
        public object ApiErrors { get; set; } 
    }

    public class DropPointData
    {
        [JsonPropertyName("geometries")]
        public Geometries Geometries { get; set; }

        [JsonPropertyName("revGeocode")]
        public RevGeocode RevGeocode { get; set; }

        [JsonPropertyName("routeInfo")]
        public object RouteInfo { get; set; } 
    }

    public class Geometries
    {
        [JsonPropertyName("distance")]
        public object Distance { get; set; } 

        [JsonPropertyName("nearestLat")]
        public double NearestLat { get; set; }

        [JsonPropertyName("nearestLon")]
        public double NearestLon { get; set; }

        [JsonPropertyName("layerId")]
        public object LayerId { get; set; } 

        [JsonPropertyName("geometry")]
        public object Geometry { get; set; } 
    }

    public class RevGeocode
    {
        [JsonPropertyName("items")]
        public RevGeocodeItem[] Items { get; set; }
    }

    public class RevGeocodeItem
    {
        [JsonPropertyName("mapView")]
        public MapView MapView { get; set; }

        [JsonPropertyName("address")]
        public Address Address { get; set; }

        [JsonPropertyName("access")]
        public AccessPoint[] Access { get; set; }

        [JsonPropertyName("distance")]
        public int Distance { get; set; }

        [JsonPropertyName("houseNumberType")]
        public string HouseNumberType { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("position")]
        public Position Position { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("resultType")]
        public string ResultType { get; set; }
    }

    public class MapView
    {
        [JsonPropertyName("east")]
        public double East { get; set; }

        [JsonPropertyName("south")]
        public double South { get; set; }

        [JsonPropertyName("north")]
        public double North { get; set; }

        [JsonPropertyName("west")]
        public double West { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("countryCode")]
        public string CountryCode { get; set; }

        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonPropertyName("district")]
        public string District { get; set; }

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; }

        [JsonPropertyName("county")]
        public string County { get; set; }

        [JsonPropertyName("houseNumber")]
        public string HouseNumber { get; set; }

        [JsonPropertyName("stateCode")]
        public string StateCode { get; set; }

        [JsonPropertyName("label")]
        public string Label { get; set; }

        [JsonPropertyName("countryName")]
        public string CountryName { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }
    }

    public class AccessPoint
    {
        [JsonPropertyName("lng")]
        public double Lng { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }
    }

    public class Position
    {
        [JsonPropertyName("lng")]
        public double Lng { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }
    }
}
