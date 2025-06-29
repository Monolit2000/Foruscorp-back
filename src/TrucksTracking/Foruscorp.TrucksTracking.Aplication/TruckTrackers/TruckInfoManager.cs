using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers
{
    public class TruckInfoManager : IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<TruckInfoManager> _logger;
        private readonly ConcurrentDictionary<string, TruckInfoUpdate> _truckInfoUpdates = new();
        private readonly Timer _refreshTimer;

        public TruckInfoManager(IServiceScopeFactory scopeFactory, ILogger<TruckInfoManager> logger)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _refreshTimer = new Timer(_ => RefreshTruckInfoAsync().GetAwaiter().GetResult(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
            _logger.LogInformation("TruckInfoManager initialized with refresh timer set to 5 minutes.");
        }

        public bool UpdateTruckIFuelnfoIfChanged(TruckInfoUpdate newUpdate)
        {
            if (newUpdate == null)
            {
                return false;
            }

            if (_truckInfoUpdates.TryGetValue(newUpdate.TruckId, out var existingUpdate))
            {
                var existingFuelPercentse = existingUpdate.fuelPercents;
                if (existingUpdate.fuelPercents != newUpdate.fuelPercents)
                {
                    _truckInfoUpdates[newUpdate.TruckId] = newUpdate;
                    return true;
                }
                return false;
            }

            _truckInfoUpdates.TryAdd(newUpdate.TruckId, newUpdate);
            return true;
        }


        public bool UpdateTruckLocationInfoIfChanged(TruckInfoUpdate newUpdate)
        {
            if (newUpdate == null)
            {
                return false;
            }


            if (_truckInfoUpdates.TryGetValue(newUpdate.TruckId, out var existingUpdate))
            {
                var previousLatitude = existingUpdate.Latitude ;
                if (existingUpdate.Latitude != newUpdate.Latitude && existingUpdate.Longitude != newUpdate.Longitude)
                {
                    _truckInfoUpdates[newUpdate.TruckId] = newUpdate;
                    return true;
                }
                return false;
            }

            _truckInfoUpdates.TryAdd(newUpdate.TruckId, newUpdate);
            return true;
        }




        public IReadOnlyCollection<TruckInfoUpdate> GetAllTruckInfo()
        {
            var truckInfoList = _truckInfoUpdates.Values.ToList();
            return truckInfoList;
        }

        private async Task RefreshTruckInfoAsync()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ITuckTrackingContext>();

                var truckIds = await context.TruckTrackers
                    .Select(tt => tt.TruckId.ToString())
                    .ToListAsync();

                var inactiveKeys = _truckInfoUpdates.Keys.Where(k => !truckIds.Contains(k)).ToList();
                foreach (var key in inactiveKeys)
                {
                    if (_truckInfoUpdates.TryRemove(key, out _))
                    {
                        _logger.LogInformation("Removed inactive truck from TruckInfoManager. TruckId: {TruckId}", key);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing active trucks in TruckInfoManager.");
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Disposing TruckInfoManager and stopping refresh timer.");
            _refreshTimer?.Dispose();
        }
    }
}