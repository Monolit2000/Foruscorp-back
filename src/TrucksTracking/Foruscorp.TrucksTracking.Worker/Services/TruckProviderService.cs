using Foruscorp.TrucksTracking.Worker.Contauct;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Foruscorp.TrucksTracking.Worker.Services
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

        public async Task<VehicleStatsResponse> GetVehicleStatsFeedAsync(List<string> vehicleIds = null, DateTime historiTimeSpun = default, CancellationToken cancellationToken = default)
        {
            try
            {
                var types = new[] { "gps", "fuelPercents", "engineStates" };
                var queryParams = new Dictionary<string, string>
                {
                    { "types", string.Join(",", types) }
                };

                //if (vehicleIds != null || !vehicleIds!.Any())
                //{
                //    queryParams["vehicleIds"] = string.Join(",", vehicleIds);
                //}

                var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                var url = $"{_baseUrl}/fleet/vehicles/stats/feed?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<VehicleStatsResponse>(content, options);

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to fetch stats for vehicle from Samsara API", ex);
            }
        }

        public async Task<VehicleStatsResponse> GetHistoricalLocationsAsync(string after = null, string timeParentTagIds = null, string tagIds = null, string vehicleIds = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(after))
                {
                    queryParams["after"] = after;
                }
                if (!string.IsNullOrEmpty(timeParentTagIds))
                {
                    queryParams["timeParentTagIds"] = timeParentTagIds;
                }
                if (!string.IsNullOrEmpty(tagIds))
                {
                    queryParams["tagIds"] = tagIds;
                }
                if (!string.IsNullOrEmpty(vehicleIds))
                {
                    queryParams["vehicleIds"] = vehicleIds;
                }

                var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                var url = $"{_baseUrl}/fleet/vehicles/locations?{queryString}";

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
                throw new Exception("Failed to fetch historical locations from Samsara API", ex);
            }
        }

        public async Task<VehicleStatsResponse> GetHistoricalStatsAsync(List<string> vehicleIds = null, DateTime historiTimeSpun = default, CancellationToken cancellationToken = default)
        {
            try
            {
                //vehicleIds.Add("281474987084961");

                var types = new[] { "gps"/*, "fuelPercents",*/ };
                var queryParams = new Dictionary<string, string>
                {
                    { "types", string.Join(",", types) }
                };
                if (vehicleIds.Count != 0)
                {
                    queryParams["vehicleIds"] = string.Join(",", vehicleIds);
                }
                if (historiTimeSpun != default)
                {
                    queryParams["startTime"] = historiTimeSpun.AddSeconds(-15).ToString("yyyy-MM-ddTHH:mm:ssZ");
                    queryParams["endTime"] = historiTimeSpun.AddSeconds(-10).ToString("yyyy-MM-ddTHH:mm:ssZ");
                }

                var queryString = string.Join("&", queryParams.Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value)}"));
                var url = $"{_baseUrl}/fleet/vehicles/stats/history?{queryString}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<VehicleStatsResponse>(content, options);

                return result;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to fetch stats for vehicle from Samsara API", ex);
            }
        }

    }
}
