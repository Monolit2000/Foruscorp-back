using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.IntegrationEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Infrastructure.Services
{
    public class TruckTrackingService : ITruckTrackingService
    {
        private readonly HttpClient _http;

        public TruckTrackingService(HttpClient http)
        {
            _http = http;
        }

        public async Task<NearFuelStationPlanDto> GetNearFuelStationPlanAsync(Guid truckId, CancellationToken cancellationToken = default)
        {
            var requestUri = $"http://truckstracking-api:5001/TrucksTracking/GetNearFuelStationPlan/{truckId}/route";

            var dto = await _http.GetFromJsonAsync<NearFuelStationPlanDto>(requestUri, cancellationToken);
            if (dto == null)
            {
                throw new InvalidOperationException(
                    $"TruckTracking API returned no route for TruckId '{truckId}'.");
            }

            return dto;
        }

        public async Task<int> GetNearFuelStationBonusAsync(Guid truckId, DateTime transactionTime, double Quantity, double tankCapacity = 200, CancellationToken cancellationToken = default)
        {
            var request = new NearFuelPlanRequest
            {
                TruckId = truckId,
                TransactionTime = transactionTime,
                Quantity = Quantity,    
                TankCapacity = tankCapacity
            };

            var response = await _http.PostAsJsonAsync("http://truckstracking-api:5001/Transaction/near-fuel-plan-bonus", request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException($"TruckTracking API returned error {response.StatusCode}: {content}");
            }

            var bonusIndex = await response.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);
            if (bonusIndex == null)
                throw new InvalidOperationException("TruckTracking API returned null bonus index.");

            return bonusIndex;
        }
    }
}
