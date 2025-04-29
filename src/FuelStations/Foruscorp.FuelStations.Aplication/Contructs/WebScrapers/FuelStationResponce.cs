using Newtonsoft.Json;
using System.Globalization;

namespace Foruscorp.FuelStations.Aplication.Contructs.WebScrapers
{

    public class FuelStationResponce
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("latitude")]
        public string Latitude { get; set; } = "0";

        [JsonProperty("longitude")]
        public string Longitude { get; set; } = "0";

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("price")]
        public object Price { get; set; } 

        [JsonProperty("discount")]
        public object Discount { get; set; } 

        [JsonProperty("price_after_discount")]
        public string PriceAfterDiscount { get; set; } 

        [JsonProperty("distance_to_location")]
        public object DistanceToLocation { get; set; } 

        [JsonProperty("image")]
        public FuelStationImage Image { get; set; }

        [JsonProperty("route")]
        public int Route { get; set; }

        // Helper methods to safely access values
        public string? GetPriceAsString() => Price switch
        {
            decimal d => d.ToString("F3"), 
            double d => d.ToString(CultureInfo.InvariantCulture), 
            string s when s == "N/A" => "N/A",
            string s => s,                
            _ => "N/A"
        };

        public string? GetDiscountAsStringl() => Discount switch
        {
            decimal d => d.ToString("F3"),
            double d => d.ToString(CultureInfo.InvariantCulture),
            string s when s == "N/A" => "N/A",
            string s => s,
            _ => "N/A"
        };


        public string? GetDistanceToLocationAsString() => DistanceToLocation switch
        {
            decimal d => d.ToString("F3"),
            double d => d.ToString("F3"),
            string s when s == "N/A" => "N/A",
            string s => s,
            _ => null
        };


    


        //public decimal? GetPriceAfterDiscountAsDecimal() => PriceAfterDiscount switch
        //{
        //    decimal d => d,
        //    double d => (decimal)d,
        //    string s when decimal.TryParse(s, out var result) => result,
        //    string s when s == "N/A" => null,
        //    _ => null
        //};
    }

    public class FuelStationImage
    {
        [JsonProperty("tile")]
        public string Tile { get; set; }
    }

}
