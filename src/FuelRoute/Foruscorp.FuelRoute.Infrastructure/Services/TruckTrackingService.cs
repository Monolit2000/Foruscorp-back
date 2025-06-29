using Foruscorp.FuelRoutes.Aplication.Contruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Infrastructure.Services
{
    public class TruckTrackingService : ITruckTrackingService
    {
        private readonly HttpClient _http;

        public TruckTrackingService(HttpClient http)
        {
            _http = http;
        }

        public async Task<TrackedRouteDto> GetRouteAsync(Guid truckId, CancellationToken cancellationToken = default)
        {
            // Формируем путь запроса без ведущего слеша — BaseAddress уже настроен
            var requestUri = $"http://truckstracking-api:5001/TrucksTracking/{truckId}/route";

            // Делаем GET и сразу десериализуем JSON в TrackedRouteDto
            var dto = await _http.GetFromJsonAsync<TrackedRouteDto>(requestUri, cancellationToken);
            if (dto == null)
            {
                throw new InvalidOperationException(
                    $"TruckTracking API returned no route for TruckId '{truckId}'.");
            }

            return dto;
        }
    }
}
