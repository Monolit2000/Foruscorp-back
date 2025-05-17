using Microsoft.AspNetCore.SignalR;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Aplication.Contruct.TruckProvider;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace Foruscorp.TrucksTracking.API.Realtime
{
    internal sealed class TruckLocationUpdater(
        IHubContext<TruckHub, ITruckLocationUpdateClient> hubContext,
        ILogger<TruckLocationUpdater> logger,
        ActiveTruckManager activeTruckManager,
        IServiceScopeFactory scopeFactory,
        ITruckProviderService truckProviderService,
        IMemoryCache memoryCache) : BackgroundService
    {

        private readonly Random _random = new();

        private const string TrackersCacheKey = "TruckTrackersCache";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await UpdateTruckLocation();
                    await Task.Delay(1500, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while updating truck locations or delaying execution.");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }


        private async Task UpdateTruckLocation()
        {
            var trucks = await UpdateTrucksStat();

            if (!trucks.Any())
                return;

            foreach (var truck in trucks)
            {
                //await hubContext.Clients.All.ReceiveTruckLocationUpdate(update);


                var locationUpdate = new TruckLocationUpdate(
                    truck.TruckId,
                    truck.TruckName,
                    truck.Longitude,
                    truck.Latitude,
                    truck.Time,
                    truck.HeadingDegrees);

                var fuelUpdate = new TruckFuelUpdate(
                    truck.TruckId,
                    truck.fuelPercents);

                await hubContext.Clients.Group(truck.TruckId.ToString()).ReceiveTruckLocationUpdate(locationUpdate);
                await hubContext.Clients.Group(truck.TruckId.ToString()).ReceiveTruckFuelUpdate(fuelUpdate);

                logger.LogInformation("Updated {Tracker} location to Longitude: {newLocation.Longitude}, Longitude: {newLocation.Latitude}",
                    truck, truck.Longitude, truck.Latitude);
            }
        }



        private async Task<List<TruckStatsUpdate>> UpdateTrucksStat()
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITuckTrackingContext>();

            var trackers = await memoryCache.GetOrCreateAsync(TrackersCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5); 

                return await context.TruckTrackers
                    .AsNoTracking()
                    //.Where(t => activeTruckManager.GetAllTrucks().Contains(t.TruckId.ToString()))
                    .Select(t => new { t.TruckId, t.ProviderTruckId })
                    .ToListAsync();
            });


            if (!trackers.Any())
                return new List<TruckStatsUpdate>();

            var providerIds = trackers.Select(t => t.ProviderTruckId).ToList();

            var response = await truckProviderService.GetVehicleStatsFeedAsync();

            var updates = response.Data
                .Where(vs => providerIds.Contains(vs.Id) || vs.EngineStates?.Any(es => es.Value == "On") == true)
                .Join(trackers,
                    vs => vs.Id, 
                    t => t.ProviderTruckId, 
                    (vs, t) => new TruckStatsUpdate(
                        t.TruckId.ToString(), 
                        vs.Name,
                        vs.Gps.FirstOrDefault()?.Longitude ?? 0,
                        vs.Gps.FirstOrDefault()?.Latitude ?? 0,
                        vs.Gps.FirstOrDefault()?.Time,
                        vs.Gps.FirstOrDefault()?.HeadingDegrees ?? 0,
                        vs.FuelPercents.FirstOrDefault()?.Value ?? 0))
                .ToList();

            return updates;
        }
    }

    public sealed record TruckStatsUpdate(
        string TruckId, 
        string TruckName,
        double Longitude,
        double Latitude, 
        string Time,
        double HeadingDegrees, 
        double fuelPercents);
}





//using Microsoft.AspNetCore.SignalR;
//using Foruscorp.TrucksTracking.Domain.Trucks;
//using Foruscorp.TrucksTracking.Aplication.Contruct;
//using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
//using Microsoft.EntityFrameworkCore;
//using Foruscorp.TrucksTracking.Aplication.Contruct.TruckProvider;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Collections.Concurrent;

//namespace Foruscorp.TrucksTracking.API.Realtime
//{
//    internal sealed class TruckLocationUpdater(
//        IHubContext<TruckHub, ITruckLocationUpdateClient> hubContext,
//        ILogger<TruckLocationUpdater> logger,
//        ActiveTruckManager activeTruckManager,
//        IServiceScopeFactory scopeFactory,
//        ITruckProviderService truckProviderService) : BackgroundService
//    {
//        private readonly Random _random = new();
//        // Очередь для хранения обновлений с номером тика, когда они были добавлены
//        private readonly ConcurrentQueue<(TruckLocationUpdate Update, int TickCount)> _updateQueue = new();
//        // Глобальный счетчик тиков
//        private int _globalTickCount = 0;

//        // Основной метод выполнения фоновой службы
//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                await UpdateTruckLocation();
//                await Task.Delay(1500, stoppingToken);
//            }
//        }

//        // Метод обновления местоположения грузовиков
//        private async Task UpdateTruckLocation()
//        {
//            _globalTickCount++;

//            // Получение данных о грузовиках
//            var trucks = await UpdateTrucksStat();

//            if (trucks.Any())
//            {
//                foreach (var truck in trucks)
//                {
//                    // Создание обновления с координатами
//                    var update = new TruckLocationUpdate(
//                        truck.Name,
//                        truck.Gps.FirstOrDefault()?.Longitude ?? 0,
//                        truck.Gps.FirstOrDefault()?.Latitude ?? 0,
//                        truck.Gps.FirstOrDefault()?.Time);

//                    // Добавление обновления в очередь с текущим номером тика
//                    _updateQueue.Enqueue((update, _globalTickCount));

//                    logger.LogInformation("Добавлено в очередь {Truck} местоположение: долгота {Longitude}, широта {Latitude} на тике {Tick}",
//                        truck.Name, update.Longitude, update.Latitude, _globalTickCount);
//                }
//            }

//            // Проверка и отправка обновлений, которые находятся в очереди 3 тика
//            while (_updateQueue.TryPeek(out var item) && _globalTickCount >= item.TickCount + 3)
//            {
//                if (_updateQueue.TryDequeue(out var dequeuedItem))
//                {
//                    var update = dequeuedItem.Update;
//                    // Отправка обновления клиентам в соответствующей группе
//                    await hubContext.Clients.Group(update.TruckId.ToString()).ReceiveTruckLocationUpdate(update);
//                    logger.LogInformation("GPS data = {Time}  Отправлено обновление местоположения {Truck} клиентам после задержки в 3 тика", update.Time, update.TruckId);
//                }
//            }
//        }

//        private async Task<List<VehicleStat>> UpdateTrucksStat()
//        {
//            using var scope = scopeFactory.CreateScope();
//            var context = scope.ServiceProvider.GetRequiredService<ITuckTrackingContext>();

//            var trackers = await context.TruckTrackers
//                .AsNoTracking()
//                .Where(t => activeTruckManager.GetAllTrucks().Contains(t.TruckId.ToString()))
//                .Select(t => t.ProviderTruckId)
//                .ToListAsync();

//            if (!trackers.Any())
//                return new List<VehicleStat>();

//            // Запрос данных о грузовиках
//            var response = await truckProviderService.GetVehicleStatsFeedAsync();

//            return response.Data
//                .Where(vs => trackers.Contains(vs.Id) ||
//                             vs.EngineStates?.Any(es => es.Value == "On") == true)
//                .ToList();
//        }

//        private GeoPoint CalculateNewLocation(GeoPoint currentLocation)
//        {
//            const decimal MaxCoordinateChange = 0.01m;
//            var random = _random;

//            double change = (double)MaxCoordinateChange;
//            decimal latFactor = (decimal)(random.NextDouble() * change * 2 - change);
//            decimal lonFactor = (decimal)(random.NextDouble() * change * 2 - change);

//            decimal latChange = currentLocation.Latitude * latFactor;
//            decimal lonChange = currentLocation.Longitude * lonFactor;

//            decimal newLatitude = Math.Max(-90m, Math.Min(90m, currentLocation.Latitude + latChange));
//            decimal newLongitude = Math.Max(-180m, Math.Min(180m, currentLocation.Longitude + lonChange));

//            newLatitude = Math.Round(newLatitude, 6);
//            newLongitude = Math.Round(newLongitude, 6);

//            return new GeoPoint(newLatitude, newLongitude);
//        }
//    }

//    public class TruckLocationDto
//    {
//        public string TruckId { get; }
//        public string TruckName { get; }
//        public DateTime TimeSpan { get; }
//        public decimal Longitude { get; }
//        public decimal Latitude { get; }
//        public TruckLocationDto(string truckId, decimal longitude, decimal latitude)
//        {
//            TruckId = truckId;
//            Longitude = longitude;
//            Latitude = latitude;
//        }
//    }

//    internal sealed class TruckLocationUpdateOptions
//    {
//        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(1);
//    }
//}