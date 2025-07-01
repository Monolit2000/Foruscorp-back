using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTime;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using Foruscorp.TrucksTracking.Aplication.Contruct.TruckProvider;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTrackerIfChanged;
using Foruscorp.TrucksTracking.Domain.Trucks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;

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
        private const string FuelCacheKey = "TruckFuelCache"; 

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    //await UpdateTruckLocation();
                    await Task.Delay(1000, stoppingToken);
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

                if (truck == null)
                {
                    break;
                }

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

                //logger.LogInformation("Updated {@Tracker} location",
                //    truck);

                //logger.LogInformation("Updated {@FuelUpdate} location",
                //    fuelUpdate);
            }
        }



        private async Task<List<TruckInfoUpdate>> UpdateTrucksStat()
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITuckTrackingContext>();

            var trackers = await memoryCache.GetOrCreateAsync(TrackersCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
                return await context.TruckTrackers
                    .AsNoTracking()
                    .Select(t => new { t.TruckId, t.ProviderTruckId })
                    .ToListAsync();
            });

            if (trackers == null || !trackers.Any())
            {
                logger.LogWarning("No trackers found in cache or database.");
                return new List<TruckInfoUpdate>();
            }

            trackers = trackers.Where(t => t.ProviderTruckId != null).ToList();

            var providerIds = trackers.Select(t => t.ProviderTruckId).ToList();

            //Call samsara API
            var response = await truckProviderService.GetVehicleStatsFeedAsync();

            if (response == null || response.Data == null)
            {
                logger.LogWarning("Vehicle stats feed response or data is null.");
                return new List<TruckInfoUpdate>();
            }

            var filteredData = response.Data
                .Where(vs => vs.Id != null && (providerIds.Contains(vs.Id) || vs.EngineStates?.Any(es => es.Value == "On") == true))
                .ToList();

            if (!filteredData.Any())
            {
                logger.LogWarning("No vehicle stats matched the filter criteria.");
                return new List<TruckInfoUpdate>();
            }

            var updates = filteredData
                .Join(trackers,
                    vs => vs.Id,
                    t => t.ProviderTruckId,
                    (vs, t) =>
                    {
                        var gps = vs.Gps != null ? vs.Gps.FirstOrDefault() : null;
                        var fuel = vs.FuelPercents != null ? vs.FuelPercents.FirstOrDefault() : null;

                        return new TruckInfoUpdate(
                            t.TruckId.ToString(),
                            vs.Name ?? "Unknown",
                            gps?.Longitude ?? 0,
                            gps?.Latitude ?? 0,
                            gps?.Time ?? DateTime.UtcNow.ToString(),
                            gps?.HeadingDegrees ?? 0,
                            fuel?.Value ?? 0,
                            gps?.ReverseGeo?.FormattedLocation ?? "Unknown",
                            vs.EngineStates?.FirstOrDefault());
                    })
                .ToList();

            logger.LogInformation("Retrieved {UpdateCount} truck stat updates.", updates.Count);

            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var command = new UpdateTruckTrackerIfChangedCommand { TruckStatsUpdates = updates };

            // fire-and-forget
            _ = Task.Run(async () =>
            {
                // создаём новый scope именно для отправки команды
                using var innerScope = scopeFactory.CreateScope();
                var sender = innerScope.ServiceProvider.GetRequiredService<ISender>();

                try
                {
                    await sender.Send(command);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Ошибка при асинхронной отправке UpdateTruckTrackerIfChangedCommand");
                }
            });

            return updates;
        }

        //private async Task ProcessFuelChangesAsync(
        //    List<TruckInfoUpdate> updates,
        //    List<VehicleStat> filteredData, // Replace with actual type of response.Data
        //    List<TruckTracker> trackers, // Replace with actual type of trackers
        //    ITuckTrackingContext context)
        //{
        //    // Get all previous fuel levels from the database in one query
        //    var truckIds = updates.Select(u => Guid.Parse(u.TruckId)).ToList();
        //    var latestFuelRecords = await context.TruckTrackers
        //        .Where(f => truckIds.Contains(f.Id))
        //        .GroupBy(f => f.TruckId)
        //        .Select(g => new
        //        {
        //            TruckId = g.Key,
        //            LatestFuel = g.OrderByDescending(f => f.RecordedAt).Select(f => f.NewFuelLevel).FirstOrDefault()
        //        })
        //        .ToDictionaryAsync(f => f.TruckId, f => f.LatestFuel);

        //    // Get or set cache for all trucks
        //    var fuelCacheKeys = truckIds.Select(id => $"{FuelCacheKey}_{id}").ToList();
        //    var cachedFuelValues = new Dictionary<string, decimal>();

        //    foreach (var truckId in truckIds)
        //    {
        //        var cacheKey = $"{FuelCacheKey}_{truckId}";
        //        var cachedFuel = await memoryCache.GetOrCreateAsync(cacheKey, entry =>
        //        {
        //            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        //            return Task.FromResult(latestFuelRecords.TryGetValue(truckId, out var fuel) ? fuel : 0m);
        //        });
        //        cachedFuelValues[cacheKey] = cachedFuel;
        //    }

        //    // Detect fuel changes and trigger HandleFuelChangeAsync.Tasks
        //    var fuelChangeTasks = updates
        //        .Where(u =>
        //        {
        //            var cacheKey = $"{FuelCacheKey}_{u.TruckId}";
        //            return cachedFuelValues.TryGetValue(cacheKey, out var previousFuel) && previousFuel != u.fuelPercents;
        //        })
        //        .Select(async u =>
        //        {
        //            var gps = filteredData
        //                .Join(trackers, vs => vs.Id, t => t.ProviderTruckId, (vs, t) => new { vs, t })
        //                .Where(x => x.t.TruckId.ToString() == u.TruckId)
        //                .Select(x => x.vs.Gps?.FirstOrDefault())
        //                .FirstOrDefault();

        //            //await HandleFuelChangeAsync(
        //            //    Guid.Parse(u.TruckId),
        //            //    cachedFuelValues[$"{FuelCacheKey}_{u.TruckId}"],
        //            //    u.fuelPercents,
        //            //    gps != null ? new GeoPoint(gps.Latitude, gps.Longitude) : null,
        //            //    gps?.ReverseGeo?.FormattedLocation);
        //        })
        //        .ToList();

        //    // Await all fuel change tasks
        //    await Task.WhenAll(fuelChangeTasks);
        //}
    }
}
