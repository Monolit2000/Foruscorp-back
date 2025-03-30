using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Aplication.Contructs.WebScrapers
{
    public class FuelStationResponce
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("latitude")]
        public string Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public string Longitude { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("address")]
        public string Address { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("price")]
        public string? Price { get; set; }

        [JsonPropertyName("discount")]
        public string? Discount { get; set; }

        [JsonPropertyName("price_after_discount")]
        public string PriceAfterDiscount { get; set; }

        [JsonPropertyName("distance_to_location")]
        public string DistanceToLocation { get; set; }

        [JsonPropertyName("image")]
        public FuelStationImage Image { get; set; }

        [JsonPropertyName("route")]
        public int Route { get; set; }
    }

    public class FuelStationImage
    {
        [JsonPropertyName("tile")]
        public string Tile { get; set; }
    }

}
