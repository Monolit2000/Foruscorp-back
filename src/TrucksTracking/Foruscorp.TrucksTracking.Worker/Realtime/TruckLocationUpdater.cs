using Foruscorp.TrucksTracking.Worker.Contauct;
using Foruscorp.TrucksTracking.Worker.Infrastructure.Database;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Channels;

namespace Foruscorp.TrucksTracking.Worker.Realtime
{
    public sealed class TruckLocationUpdater : BackgroundService
    {
        private readonly ILogger<TruckLocationUpdater> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITruckProviderService _providerService;
        private readonly IMemoryCache _memoryCache;
        private readonly IPublishEndpoint _publishEndpoint;
        private const string TrackersCacheKey = "TruckTrackersCache";

        public TruckLocationUpdater(
            ILogger<TruckLocationUpdater> logger,
            IServiceScopeFactory scopeFactory,
            ITruckProviderService providerService,
            IMemoryCache memoryCache,
            IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _providerService = providerService;
            _memoryCache = memoryCache;
            _publishEndpoint = publishEndpoint;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return PollLoopAsync(stoppingToken);
        }

        private async Task PollLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var updates = await FetchUpdatesAsync(token);
                    if (updates.Any())
                    {
                        await BroadcastUpdatesAsync(updates, token);
                        //EnqueueUpdates(updates, token);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2), token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during polling loop");
                    await Task.Delay(TimeSpan.FromSeconds(5), token);
                }
            }
        }

        private async Task<List<TruckInfoUpdate>> FetchUpdatesAsync(CancellationToken token)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TruckTrackerWorkerContext>();

            var trackers = await _memoryCache.GetOrCreateAsync(
                TrackersCacheKey,
                async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
                    return await context.TruckTrackers
                        .AsNoTracking()
                        .Select(t => new { t.TruckId, t.ProviderTruckId })
                        .ToListAsync(token);
                });

            if (trackers == null || !trackers.Any())
            {
                _logger.LogWarning("No trackers found.");
                return new List<TruckInfoUpdate>();
            }

            var providerIds = trackers
                .Where(t => !string.IsNullOrEmpty(t.ProviderTruckId))
                .Select(t => t.ProviderTruckId)
                .ToHashSet();

            var response = await _providerService.GetVehicleStatsFeedAsync();
            if (response?.Data == null)
            {
                _logger.LogWarning("External stats feed is null.");
                return new List<TruckInfoUpdate>();
            }

            var filtered = response.Data
                .Where(v => v.Id != null && (providerIds.Contains(v.Id) ||
                                              v.EngineStates?.Any(es => es.Value == "On") == true))
                .ToList();

            if (!filtered.Any())
            {
                _logger.LogInformation("No matching vehicle stats.");
                return new List<TruckInfoUpdate>();
            }

            var updates = filtered
                .Join(trackers,
                      vs => vs.Id,
                      t => t.ProviderTruckId,
                      (vs, t) =>
                      {
                          var gps = vs.Gps?.FirstOrDefault();
                          var fuel = vs.FuelPercents?.FirstOrDefault();
                          return new TruckInfoUpdate(
                              t.TruckId.ToString(),
                              vs.Name ?? "Unknown",
                              gps?.Longitude ?? 0,
                              gps?.Latitude ?? 0,
                              gps?.Time ?? DateTime.UtcNow.ToString("o"),
                              gps?.HeadingDegrees ?? 0,
                              fuel?.Value ?? 0,
                              gps?.ReverseGeo?.FormattedLocation ?? "Unknown",
                              vs.EngineStates?.FirstOrDefault());
                      })
                .ToList();

            _logger.LogInformation("Fetched {Count} updates.", updates.Count);
            return updates;
        }



        private async Task BroadcastUpdatesAsync(
            List<TruckInfoUpdate> updates,
            CancellationToken token)
        {
            foreach (var u in updates)
            {
                try
                {
                    //var group = u.TruckId.ToString();
                    //await _hubContext.Clients.Group(group).ReceiveTruckLocationUpdate(
                    //    new TruckLocationUpdate(
                    //        u.TruckId,
                    //        u.TruckName,
                    //        u.Longitude,
                    //        u.Latitude,
                    //        u.Time,
                    //        u.HeadingDegrees));

                    //await _hubContext.Clients.Group(group).ReceiveTruckFuelUpdate(
                    //    new TruckFuelUpdate(u.TruckId, u.fuelPercents));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to broadcast update for {TruckId}", u.TruckId);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Complete all writers

            await base.StopAsync(cancellationToken);
        }


    }
}
