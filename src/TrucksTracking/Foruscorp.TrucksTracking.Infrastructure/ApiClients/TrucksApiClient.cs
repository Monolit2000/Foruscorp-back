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
    /// HTTP клиент для работы с Trucks API
    /// </summary>
    public class TrucksApiClient : ITrucksApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<TrucksApiClient> _logger;

        public TrucksApiClient(HttpClient httpClient, ILogger<TrucksApiClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TruckInfoDto> GetTruckInfoAsync(Guid truckId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("🚛 Запрашиваем информацию о грузовике {TruckId}", truckId);
                
                var response = await _httpClient.GetFromJsonAsync<TruckInfoDto>(
                    $"/api/trucks/{truckId}", 
                    cancellationToken);

                if (response != null)
                {
                    _logger.LogDebug("✅ Получена информация о грузовике {TruckId}: {TruckNumber}", 
                        truckId, response.Number);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении информации о грузовике {TruckId}", truckId);
                throw;
            }
        }

        public async Task<DriverInfoDto> GetDriverInfoAsync(Guid driverId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("👤 Запрашиваем информацию о водителе {DriverId}", driverId);
                
                var response = await _httpClient.GetFromJsonAsync<DriverInfoDto>(
                    $"/api/drivers/{driverId}", 
                    cancellationToken);

                if (response != null)
                {
                    _logger.LogDebug("✅ Получена информация о водителе {DriverId}: {DriverName}", 
                        driverId, $"{response.FirstName} {response.LastName}");
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении информации о водителе {DriverId}", driverId);
                throw;
            }
        }

        public async Task<CompanyInfoDto> GetCompanyInfoAsync(Guid companyId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("🏢 Запрашиваем информацию о компании {CompanyId}", companyId);
                
                var response = await _httpClient.GetFromJsonAsync<CompanyInfoDto>(
                    $"/api/companies/{companyId}", 
                    cancellationToken);

                if (response != null)
                {
                    _logger.LogDebug("✅ Получена информация о компании {CompanyId}: {CompanyName}", 
                        companyId, response.Name);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении информации о компании {CompanyId}", companyId);
                throw;
            }
        }

        public async Task<List<TruckInfoDto>> GetTrucksInfoAsync(List<Guid> truckIds, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("🚛📋 Запрашиваем информацию о {TruckCount} грузовиках", truckIds.Count);
                
                var response = await _httpClient.PostAsJsonAsync(
                    "/api/trucks/batch", 
                    truckIds, 
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var trucks = await response.Content.ReadFromJsonAsync<List<TruckInfoDto>>(cancellationToken);

                _logger.LogDebug("✅ Получена информация о {ReceivedCount} грузовиках из {RequestedCount}", 
                    trucks?.Count ?? 0, truckIds.Count);

                return trucks ?? new List<TruckInfoDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении информации о грузовиках (batch)");
                throw;
            }
        }

        public async Task<List<FuelHistoryDto>> GetTruckFuelHistoryAsync(Guid truckId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("⛽ Запрашиваем историю заправок грузовика {TruckId}", truckId);
                
                var query = $"/api/trucks/{truckId}/fuel-history";
                var parameters = new List<string>();

                if (from.HasValue)
                    parameters.Add($"from={from.Value:yyyy-MM-ddTHH:mm:ss}");
                
                if (to.HasValue)
                    parameters.Add($"to={to.Value:yyyy-MM-ddTHH:mm:ss}");

                if (parameters.Count > 0)
                    query += "?" + string.Join("&", parameters);

                var response = await _httpClient.GetFromJsonAsync<List<FuelHistoryDto>>(
                    query, 
                    cancellationToken);

                _logger.LogDebug("✅ Получена история заправок грузовика {TruckId}: {HistoryCount} записей", 
                    truckId, response?.Count ?? 0);

                return response ?? new List<FuelHistoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении истории заправок грузовика {TruckId}", truckId);
                throw;
            }
        }

        public async Task<TruckEfficiencyDto> GetTruckEfficiencyAsync(Guid truckId, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("📊 Запрашиваем эффективность грузовика {TruckId}", truckId);
                
                var response = await _httpClient.GetFromJsonAsync<TruckEfficiencyDto>(
                    $"/api/trucks/{truckId}/efficiency", 
                    cancellationToken);

                if (response != null)
                {
                    _logger.LogDebug("✅ Получена эффективность грузовика {TruckId}: {Efficiency} MPG", 
                        truckId, response.AverageMilesPerGallon);
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Ошибка при получении эффективности грузовика {TruckId}", truckId);
                throw;
            }
        }
    }
}
