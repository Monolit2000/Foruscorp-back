using Foruscorp.FuelStations.Aplication.Contructs.Services;
using Foruscorp.FuelStations.Aplication.Contructs.Services.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Foruscorp.FuelStations.Infrastructure.Services
{
    public class LovesApiService : ILovesApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LovesApiService> _logger;
        private const string ApiUrl = "https://www.loves.com/api/fetch_stores";

        public LovesApiService(HttpClient httpClient, ILogger<LovesApiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<LovesApiResponseModel?> GetStoresAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Fetching stores from Love's API");

                var response = await _httpClient.GetAsync(ApiUrl, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to fetch stores from Love's API. Status code: {StatusCode}", response.StatusCode);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                
                if (string.IsNullOrEmpty(content))
                {
                    _logger.LogWarning("Empty response received from Love's API");
                    return null;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = JsonSerializer.Deserialize<LovesApiResponseModel>(content, options);
                
                _logger.LogInformation("Successfully fetched {StoreCount} stores from Love's API", 
                    result?.Stores?.Count ?? 0);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP request failed while fetching stores from Love's API");
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize response from Love's API");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while fetching stores from Love's API");
                return null;
            }
        }
    }
}
