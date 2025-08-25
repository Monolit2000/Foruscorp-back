using Foruscorp.TrucksTracking.Aplication.Contruct.ApiClients;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Infrastructure.ApiClients
{
    /// <summary>
    /// HTTP клиент для работы с FuelStations API
    /// </summary>
    public class FuelStationsApiClient : IFuelStationsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FuelStationsApiClient> _logger;

        public FuelStationsApiClient(HttpClient httpClient, ILogger<FuelStationsApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<FuelStationInfoDto> GetFuelStationInfoAsync(Guid fuelStationId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("⛽ Запрашиваем информацию о станции {FuelStationId}", fuelStationId);
                
                var response = await _httpClient.GetFromJsonAsync<FuelStationInfoDto>(
                    $"/api/fuelstations/{fuelStationId}", 
                    cancellationToken);

                if (response != null)
                {
                    _logger.LogDebug("✅ Получена информация о станции {FuelStationId}: {StationName}", 
                        fuelStationId, response.Name);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении информации о станции {FuelStationId}", fuelStationId);
                throw;
            }
        }

        public async Task<List<FuelStationInfoDto>> GetFuelStationsInfoAsync(List<Guid> fuelStationIds, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("⛽📋 Запрашиваем информацию о {StationCount} станциях", fuelStationIds.Count);
                
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/fuelstations/batch", 
                    fuelStationIds, 
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var stations = await response.Content.ReadFromJsonAsync<List<FuelStationInfoDto>>(cancellationToken);

                _logger.LogDebug("✅ Получена информация о {ReceivedCount} станциях из {RequestedCount}", 
                    stations?.Count ?? 0, fuelStationIds.Count);

                return stations ?? new List<FuelStationInfoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении информации о станциях (batch)");
                throw;
            }
        }

        public async Task<List<FuelPriceDto>> GetFuelPricesAsync(Guid fuelStationId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("💰 Запрашиваем цены на станции {FuelStationId}", fuelStationId);
                
                var response = await _httpClient.GetFromJsonAsync<List<FuelPriceDto>>(
                    $"/api/fuelstations/{fuelStationId}/prices", 
                    cancellationToken);

                _logger.LogDebug("✅ Получены цены на станции {FuelStationId}: {PriceCount} позиций", 
                    fuelStationId, response?.Count ?? 0);

                return response ?? new List<FuelPriceDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении цен на станции {FuelStationId}", fuelStationId);
                throw;
            }
        }

        public async Task<List<FuelStationInfoDto>> GetNearbyStationsAsync(double latitude, double longitude, double radiusKm = 50, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("🗺️ Поиск станций рядом с координатами {Lat}, {Lng} в радиусе {Radius} км", 
                    latitude, longitude, radiusKm);
                
                var query = $"/api/fuelstations/nearby?lat={latitude}&lng={longitude}&radius={radiusKm}";
                
                var response = await _httpClient.GetFromJsonAsync<List<FuelStationInfoDto>>(
                    query, 
                    cancellationToken);

                _logger.LogDebug("✅ Найдено {StationCount} станций рядом с координатами", 
                    response?.Count ?? 0);

                return response ?? new List<FuelStationInfoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при поиске станций рядом с координатами {Lat}, {Lng}", 
                    latitude, longitude);
                throw;
            }
        }

        public async Task<FuelStationRatingDto> GetStationRatingAsync(Guid fuelStationId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("⭐ Запрашиваем рейтинг станции {FuelStationId}", fuelStationId);
                
                var response = await _httpClient.GetFromJsonAsync<FuelStationRatingDto>(
                    $"/api/fuelstations/{fuelStationId}/rating", 
                    cancellationToken);

                if (response != null)
                {
                    _logger.LogDebug("✅ Получен рейтинг станции {FuelStationId}: {Rating}/5 ({Reviews} отзывов)", 
                        fuelStationId, response.AverageRating, response.TotalReviews);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении рейтинга станции {FuelStationId}", fuelStationId);
                throw;
            }
        }

        public async Task<List<FuelPriceHistoryDto>> GetPriceHistoryAsync(Guid fuelStationId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("📈 Запрашиваем историю цен станции {FuelStationId}", fuelStationId);
                
                var query = $"/api/fuelstations/{fuelStationId}/price-history";
                var parameters = new List<string>();

                if (from.HasValue)
                    parameters.Add($"from={from.Value:yyyy-MM-ddTHH:mm:ss}");
                
                if (to.HasValue)
                    parameters.Add($"to={to.Value:yyyy-MM-ddTHH:mm:ss}");

                if (parameters.Count > 0)
                    query += "?" + string.Join("&", parameters);

                var response = await _httpClient.GetFromJsonAsync<List<FuelPriceHistoryDto>>(
                    query, 
                    cancellationToken);

                _logger.LogDebug("✅ Получена история цен станции {FuelStationId}: {HistoryCount} записей", 
                    fuelStationId, response?.Count ?? 0);

                return response ?? new List<FuelPriceHistoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении истории цен станции {FuelStationId}", fuelStationId);
                throw;
            }
        }
    }
}
