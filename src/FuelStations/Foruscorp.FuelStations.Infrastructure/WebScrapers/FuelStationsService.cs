using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;

namespace Foruscorp.FuelStations.Infrastructure.WebScrapers
{
    public class FuelStationsService : IFuelStationsService
    {
        public async Task<List<FuelStationResponce>> GetFuelStations(string bearerToken, int radius = 15, string source = "Lebanon, Kansas, США", string destination = "Lebanon, Kansas, США")
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Origin", "https://app.thefuelmap.com");
                client.DefaultRequestHeaders.Add("Referer", "https://app.thefuelmap.com/");
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

                string routeUrl = "https://fuel.fulgertransport.com/api/web/get-route";

                var payload = new
                {
                    source,
                    destination,
                    date = "2025-04-02",
                    distanceFilterId = radius.ToString(),
                    locationFilterId = new List<string>() { "AB", "AL", "AR", "AZ", "BC", "CA", "CO", "CT", "FL", "GA", "IA", "ID", "IL", "IN", "KS", "KY", "LA", "MA", "MB", "MD", "MI", "MN", "MO", "MS", "MT", "NC", "ND", "NE", "NH", "NJ", "NM", "NV", "NY", "OH", "OK", "ON", "OR", "PA", "QC", "RI", "SC", "SD", "SK", "TN", "TX", "UT", "VA", "WA", "WI", "WV", "WY" },
                    truckStopChainFilterId = new List<string>() { "Pilot", "Loves", "Road Rangers", "TA Petro", "Palmetto", "Compass", "Sapp Bros" }
                };

                var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(routeUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var fuelStations = JsonConvert.DeserializeObject<List<FuelStationResponce>>(responseBody);
                    return fuelStations ?? new List<FuelStationResponce>(); // Return empty list if null
                }
                else
                {
                    string errorBody = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Ошибка при запросе к API: {response.StatusCode}, тело ошибки: {errorBody}");
                }
            }
        }
    }
}



//using System.Text;
//using System.Text.Json;
//using System.Net.Http.Headers;
//using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;

//namespace Foruscorp.FuelStations.Infrastructure.WebScrapers
//{
//    public class FuelStationsService : IFuelStationsService 
//    {
//        public async Task<List<FuelStationResponce>> GetFuelStations(string bearerToken, int radius = 15, string source = "Lebanon, Kansas, США", string destination = "Lebanon, Kansas, США")
//        {
//            using (var client = new HttpClient())
//            {
//                client.DefaultRequestHeaders.Clear();
//                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36");
//                client.DefaultRequestHeaders.Add("Origin", "https://app.thefuelmap.com");
//                client.DefaultRequestHeaders.Add("Referer", "https://app.thefuelmap.com/");
//                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
//                client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
//                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
//                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("*/*"));

//                string routeUrl = "https://fuel.fulgertransport.com/api/web/get-route";

//                var payload = new
//                {
//                    source,
//                    destination,
//                    date = "2025-04-02",
//                    distanceFilterId = radius.ToString(),
//                    locationFilterId = new List<string>(),
//                    truckStopChainFilterId = new List<string>()
//                };

//                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

//                HttpResponseMessage response = await client.PostAsync(routeUrl, content);

//                if (response.IsSuccessStatusCode)
//                {
//                    string responseBody = await response.Content.ReadAsStringAsync();
//                    var fuelStations = JsonSerializer.Deserialize<List<FuelStationResponce>>(responseBody);
//                    return fuelStations ?? new List<FuelStationResponce>(); // Возвращаем пустой список, если десериализация вернула null
//                }
//                else
//                {
//                    string errorBody = await response.Content.ReadAsStringAsync();
//                    throw new HttpRequestException($"Ошибка при запросе к API: {response.StatusCode}, тело ошибки: {errorBody}");
//                }
//            }
//        }
//    }
//}
