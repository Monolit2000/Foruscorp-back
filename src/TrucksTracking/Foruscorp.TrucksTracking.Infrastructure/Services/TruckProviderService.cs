using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Foruscorp.TrucksTracking.Aplication.Contruct.TruckProvider;
using Foruscorp.TrucksTracking.Aplication.Contruct;

namespace Foruscorp.TrucksTracking.Infrastructure.Services
{
    public class TruckProviderService : ITruckProviderService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://api.samsara.com";
        private readonly string _apiToken;

        public TruckProviderService(IConfiguration configuration)
        {
            _apiToken = configuration["SamsaraApi:ApiToken"]
                ?? throw new ArgumentNullException("API token is missing in configuration.");
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiToken);
        }

        public async Task<VehicleResponse> GetVehiclesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/fleet/vehicles");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<VehicleResponse>(content, options);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to fetch vehicles from Samsara API", ex);
            }
        }

        public async Task<VehicleStatsResponse> GetVehicleLocationsFeedAsync(string after = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = $"{_baseUrl}/fleet/vehicles/stats/feed?types=gps";
                if (!string.IsNullOrEmpty(after))
                {
                    url += $"&after={Uri.EscapeDataString(after)}";
                }

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine("Raw JSON Response: " + content);


                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<VehicleStatsResponse>(content, options);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to fetch vehicle locations from Samsara API", ex);
            }
        }

        public async Task<VehicleStatsResponse> GetVehicleStatsFeedAsync(List<string> vehicleIds = null, string after = null, CancellationToken cancellationToken = default)
        {




            Console.WriteLine("========================================================" +_apiToken + "========================================================");



            try
            {
                var types = new[] { "gps", "fuelPercents", "engineStates" };
                var queryParams = new Dictionary<string, string>
                {
                    { "types", string.Join(",", types) }
                };
                if (vehicleIds != null || !vehicleIds!.Any())
                {
                    queryParams["vehicleIds"] = string.Join(",", vehicleIds);
                }
                if (!string.IsNullOrEmpty(after))
                {
                    queryParams["after"] = after;
                }

                var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                var url = $"{_baseUrl}/fleet/vehicles/stats/feed?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                return JsonSerializer.Deserialize<VehicleStatsResponse>(content, options);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to fetch stats for vehicle from Samsara API", ex);
            }
        }
    }
}
