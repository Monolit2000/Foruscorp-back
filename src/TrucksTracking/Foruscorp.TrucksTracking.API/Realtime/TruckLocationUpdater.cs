
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTime;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTrackerIfChanged;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Channels;

namespace Foruscorp.TrucksTracking.API.Realtime
{
    public sealed class TruckLocationUpdater : BackgroundService
    {
        private readonly IHubContext<TruckHub, ITruckLocationUpdateClient> _hubContext;
        private readonly ILogger<TruckLocationUpdater> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ITruckProviderService _providerService;
        private readonly IMemoryCache _memoryCache;

        private const int WorkerCount = 1;
        private const int ChannelCapacity = 400;
        private readonly Channel<UpdateTruckTrackerIfChangedCommand>[] _commandChannels;
        private readonly Task[] _processorTasks;

        public TruckLocationUpdater(
            IHubContext<TruckHub, ITruckLocationUpdateClient> hubContext,
            ILogger<TruckLocationUpdater> logger,
            IServiceScopeFactory scopeFactory,
            ITruckProviderService providerService,
            IMemoryCache memoryCache)
        {
            _hubContext = hubContext;
            _logger = logger;
            _scopeFactory = scopeFactory;
            _providerService = providerService;
            _memoryCache = memoryCache;

            _commandChannels = new Channel<UpdateTruckTrackerIfChangedCommand>[WorkerCount];
            _processorTasks = new Task[WorkerCount];

            // Initialize channels
            for (int i = 0; i < WorkerCount; i++)
            {
                _commandChannels[i] = Channel.CreateBounded<UpdateTruckTrackerIfChangedCommand>(
                    new BoundedChannelOptions(ChannelCapacity)
                    {
                        SingleReader = true,
                        SingleWriter = false,
                        FullMode = BoundedChannelFullMode.Wait
                    });
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Start processors
            for (int i = 0; i < WorkerCount; i++)
            {
                var reader = _commandChannels[i].Reader;
                _processorTasks[i] = Task.Run(() => ProcessCommandsAsync(reader, stoppingToken), stoppingToken);
            }

            // Start polling
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
                        EnqueueUpdates(updates, token);
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
            var context = scope.ServiceProvider.GetRequiredService<ITruckTrackingContext>();

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

        private void EnqueueUpdates(List<TruckInfoUpdate> updates, CancellationToken token)
        {

            _commandChannels[0].Writer.WriteAsync(
             new UpdateTruckTrackerIfChangedCommand { TruckStatsUpdates = updates },
             token);

            //foreach (var update in updates)
            //{
            //    int shard = Math.Abs(update.TruckId.GetHashCode()) % WorkerCount;
            //    _commandChannels[shard].Writer.WriteAsync(
            //        new UpdateTruckTrackerIfChangedCommand { TruckStatsUpdates = new List<TruckInfoUpdate> { update } },
            //        token);
            //}
        }

        private async Task BroadcastUpdatesAsync(
            List<TruckInfoUpdate> updates,
            CancellationToken token)
        {
            foreach (var u in updates)
            {
                try
                {
                    var group = u.TruckId.ToString();
                    await _hubContext.Clients.Group(group).ReceiveTruckLocationUpdate(
                        new TruckLocationUpdate(
                            u.TruckId,
                            u.TruckName,
                            u.Longitude,
                            u.Latitude,
                            u.Time,
                            u.HeadingDegrees));

                    await _hubContext.Clients.Group(group).ReceiveTruckFuelUpdate(
                        new TruckFuelUpdate(u.TruckId, u.fuelPercents));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to broadcast update for {TruckId}", u.TruckId);
                }
            }
        }

        private async Task ProcessCommandsAsync(
            ChannelReader<UpdateTruckTrackerIfChangedCommand> reader,
            CancellationToken token)
        {
            await foreach (var cmd in reader.ReadAllAsync(token))
            {
                try
                {
                    _logger.LogInformation("Processing reader for {Count} items", reader.Count);

                    using var scope = _scopeFactory.CreateScope();
                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                    await sender.Send(cmd, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing command");
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            // Complete all writers
            foreach (var channel in _commandChannels)
                channel.Writer.Complete();

            // Wait for all processors to finish
            await Task.WhenAll(_processorTasks);

            await base.StopAsync(cancellationToken);
        }

        private const string TrackersCacheKey = "TruckTrackersCache";
    }
}






//using Foruscorp.TrucksTracking.Aplication.Contruct;
//using Foruscorp.TrucksTracking.Aplication.Contruct.RealTime;
//using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
//using Foruscorp.TrucksTracking.Aplication.TruckTrackers.UpdateTruckTrackerIfChanged;
//using Foruscorp.TrucksTracking.Domain.Trucks;
//using MediatR;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Caching.Memory;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Channels;
//using System.Threading.Tasks;

//namespace Foruscorp.TrucksTracking.API.Realtime
//{
//    internal sealed class TruckLocationUpdater : BackgroundService
//    {
//        private readonly IHubContext<TruckHub, ITruckLocationUpdateClient> _hubContext;
//        private readonly ILogger<TruckLocationUpdater> _logger;
//        private readonly IServiceScopeFactory _scopeFactory;
//        private readonly ITruckProviderService _providerService;
//        private readonly IMemoryCache _memoryCache;

//         Bound channel to avoid unbounded growth
//        private readonly Channel<UpdateTruckTrackerIfChangedCommand> _commandChannel =
//            Channel.CreateBounded<UpdateTruckTrackerIfChangedCommand>(
//                new BoundedChannelOptions(100)
//                {
//                    SingleReader = true,
//                    SingleWriter = false,
//                    FullMode = BoundedChannelFullMode.Wait
//                });

//        private Task _processorTask;

//        public TruckLocationUpdater(
//            IHubContext<TruckHub, ITruckLocationUpdateClient> hubContext,
//            ILogger<TruckLocationUpdater> logger,
//            IServiceScopeFactory scopeFactory,
//            ITruckProviderService providerService,
//            IMemoryCache memoryCache)
//        {
//            _hubContext = hubContext;
//            _logger = logger;
//            _scopeFactory = scopeFactory;
//            _providerService = providerService;
//            _memoryCache = memoryCache;
//        }

//        protected override Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//             Start command processor
//            _processorTask = ProcessCommandsAsync(stoppingToken);
//             Start main polling loop
//            return PollLoopAsync(stoppingToken);
//        }

//        private async Task PollLoopAsync(CancellationToken token)
//        {
//            while (!token.IsCancellationRequested)
//            {
//                try
//                {
//                    var updates = await FetchUpdatesAsync(token);
//                    if (updates.Any())
//                    {
//                        await BroadcastUpdatesAsync(updates, token);
//                    }
//                    await Task.Delay(TimeSpan.FromSeconds(2), token);
//                }
//                catch (OperationCanceledException)
//                {
//                    break;
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Error during polling loop");
//                    await Task.Delay(TimeSpan.FromSeconds(5), token);
//                }
//            }
//        }

//        private async Task<List<TruckInfoUpdate>> FetchUpdatesAsync(CancellationToken token)
//        {
//            using var scope = _scopeFactory.CreateScope();
//            var context = scope.ServiceProvider.GetRequiredService<ITruckTrackingContext>();

//             Load trackers from cache or DB
//            var trackers = await _memoryCache.GetOrCreateAsync(
//                TrackersCacheKey,
//                async entry =>
//                {
//                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
//                    return await context.TruckTrackers
//                        .AsNoTracking()
//                        .Select(t => new { t.TruckId, t.ProviderTruckId })
//                        .ToListAsync(token);
//                });

//            if (trackers == null || !trackers.Any())
//            {
//                _logger.LogWarning("No trackers found.");
//                return new List<TruckInfoUpdate>();
//            }

//            var providerIds = trackers
//                .Where(t => !string.IsNullOrEmpty(t.ProviderTruckId))
//                .Select(t => t.ProviderTruckId)
//                .ToHashSet();

//             Call external API
//            var response = await _providerService.GetVehicleStatsFeedAsync();
//            if (response?.Data == null)
//            {
//                _logger.LogWarning("External stats feed is null.");
//                return new List<TruckInfoUpdate>();
//            }

//            var filtered = response.Data
//                .Where(v => v.Id != null && (providerIds.Contains(v.Id) ||
//                                              v.EngineStates?.Any(es => es.Value == "On") == true))
//                .ToList();

//            if (!filtered.Any())
//            {
//                _logger.LogInformation("No matching vehicle stats.");
//                return new List<TruckInfoUpdate>();
//            }

//            var updates = filtered
//                .Join(trackers,
//                      vs => vs.Id,
//                      t => t.ProviderTruckId,
//                      (vs, t) =>
//                      {
//                          var gps = vs.Gps?.FirstOrDefault();
//                          var fuel = vs.FuelPercents?.FirstOrDefault();
//                          return new TruckInfoUpdate(
//                              t.TruckId.ToString(),
//                              vs.Name ?? "Unknown",
//                              gps?.Longitude ?? 0,
//                              gps?.Latitude ?? 0,
//                              gps?.Time ?? DateTime.UtcNow.ToString("o"),
//                              gps?.HeadingDegrees ?? 0,
//                              fuel?.Value ?? 0,
//                              gps?.ReverseGeo?.FormattedLocation ?? "Unknown",
//                              vs.EngineStates?.FirstOrDefault());
//                      })
//                .ToList();

//            _logger.LogInformation("Fetched {Count} updates.", updates.Count);

//             Enqueue for database processing
//            await _commandChannel.Writer.WriteAsync(
//                new UpdateTruckTrackerIfChangedCommand { TruckStatsUpdates = updates }, token);

//            return updates;
//        }

//        private async Task BroadcastUpdatesAsync(
//            List<TruckInfoUpdate> updates,
//            CancellationToken token)
//        {
//            foreach (var u in updates)
//            {
//                try
//                {
//                    var group = u.TruckId.ToString();
//                    await _hubContext.Clients.Group(group).ReceiveTruckLocationUpdate(
//                        new TruckLocationUpdate(
//                            u.TruckId,
//                            u.TruckName,
//                            u.Longitude,
//                            u.Latitude,
//                            u.Time,
//                            u.HeadingDegrees));

//                    await _hubContext.Clients.Group(group).ReceiveTruckFuelUpdate(
//                        new TruckFuelUpdate(u.TruckId, u.fuelPercents));
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogWarning(ex, "Failed to broadcast update for {TruckId}", u.TruckId);
//                }
//            }
//        }

//        private async Task ProcessCommandsAsync(CancellationToken token)
//        {
//            await foreach (var cmd in _commandChannel.Reader.ReadAllAsync(token))
//            {
//                try
//                {
//                    using var scope = _scopeFactory.CreateScope();
//                    var sender = scope.ServiceProvider.GetRequiredService<ISender>();
//                    await sender.Send(cmd, token);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, "Error processing command");
//                }
//            }
//        }

//        public override async Task StopAsync(CancellationToken cancellationToken)
//        {
//            _commandChannel.Writer.Complete();
//            if (_processorTask != null)
//            {
//                await _processorTask;
//            }
//            await base.StopAsync(cancellationToken);
//        }

//        private const string TrackersCacheKey = "TruckTrackersCache";
//    }
//}
