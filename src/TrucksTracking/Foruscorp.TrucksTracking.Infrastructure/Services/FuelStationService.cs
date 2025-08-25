using Foruscorp.FuelStations.IntegrationEvents;
using Foruscorp.Trucks.IntegrationEvents;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Infrastructure.Services
{
    public class FuelStationService : IFuelStationService
    {
        private readonly HttpClient _http;

        public FuelStationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<FuelStationIntegrationDto> GetFuelStationInfoAsync(Guid fuelStationId, CancellationToken cancellationToken = default)
        {
            var requestUri = $"http://fuelstations-api:5002/FuelStation/{fuelStationId}/fuelStation";

            var dto = await _http.GetFromJsonAsync<FuelStationIntegrationDto>(requestUri, cancellationToken);
            if (dto == null)
            {
                throw new InvalidOperationException(
                    $"TruckTracking API returned no route for TruckId '{fuelStationId}'.");
            }

            return dto;
        }
    }
   
}
