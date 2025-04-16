using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Infrastructure.ApiClients
{
    public class TruckerPathApiClient : ITruckerPathApi
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.truckerpath.com/fleet/route/planning/v3";
        private JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public TruckerPathApiClient()
        {
            _httpClient = new HttpClient(); /*httpClient ?? throw new ArgumentNullException(nameof(httpClient));*/
        }

        public async Task<DataObject> PlanRouteAsync(GeoPoint origin, GeoPoint destinations, CancellationToken cancellationToken = default)
        {
            var requestBody = new RoutePlanningRequest
            {
                NeedWholePoint = true,
                RouteDescription = "null_1744404106187",
                IsNavigation = 0,
                WayPoints = new[]
                {
                    new WayPoint
                    {
                        OriginalPosition = new TPosition
                        {
                            Course = 360,
                            Latitude = origin.Latitude,
                            Longitude = origin.Longitude
                        },
                        Index = 0,
                        PlaceType = "address",
                        AddressName = "801, E 91st St",
                        DwellTime = 0
                    },
                    new WayPoint
                    {
                        OriginalPosition = new TPosition
                        {
                            Course = 360,
                            Latitude = destinations.Latitude,
                            Longitude = destinations.Longitude
                        },
                        Index = 1,
                        PlaceType = "address",
                        AddressName = "Tejon St",
                        DwellTime = 0
                    }
                },
                RouteOption = new RouteOption
                {
                    DepartureTime = "1744429295",
                    RoutingMode = "truck",
                    ResultLimit = 3,
                    RoutingType = "fastest",
                    RoutingOption = new RoutingOption
                    {
                        DirtRoad = true,
                        Toll = false,
                        Tunnel = false,
                        UTurns = true,
                        Ferry = true
                    },
                    SpeedProfile = "fast",
                    TruckModel = new TruckModel
                    {
                        LimitedVehicleWeight = 36.2874,
                        WeightPerAxle = 0,
                        VehicleHeight = 4.1148,
                        VehicleWidth = 2.5908,
                        VehicleLength = 19.812,
                        TruckAxleCount = 5,
                        TrailersCount = 1,
                        HazardousGoods = ""
                    }
                },
                Affected = new Affected
                {
                    LinkId = Array.Empty<string>(),
                    Area = Array.Empty<string>(),
                    Polygon = Array.Empty<string>()
                }
            };

            try
            {
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiUrl, content, cancellationToken);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();

                var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent, JsonOptions);

                if (!string.IsNullOrEmpty(apiResponse.Data))
                {
                    var data = JsonSerializer.Deserialize<DataObject>(apiResponse.Data, JsonOptions);
                    return data;
                }

                return null;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to communicate with the TruckerPath API", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to deserialize API response", ex);
            }
        }

        public async Task ChengePint(string routeId, CancellationToken cancellationToken = default)
        {
            var requestUrl = $"{ApiUrl}/{routeId}/routePoints";
            try
            {
                var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                var routePointsResponse = JsonSerializer.Deserialize<List<RoutePoint>>(responseContent, JsonOptions);
                //return routePointsResponse;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to communicate with the TruckerPath API", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to deserialize API response", ex);
            }
        }
    }
}
