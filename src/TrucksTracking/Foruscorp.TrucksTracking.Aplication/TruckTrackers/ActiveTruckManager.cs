using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Domain.Trucks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers
{
    public class ActiveTruckManager : IDisposable
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ConcurrentBag<string> _activeTruck = new();
        private readonly Timer _refreshTimer;

        public ActiveTruckManager(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            // Set up timer to refresh every 5 minutes
            _refreshTimer = new Timer(_ => SetActiveTruckList(), null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        public void AddTruck(string truckId)
        {
            if (!_activeTruck.Contains(truckId))
            {
                _activeTruck.Add(truckId);
            }
        }

        public void RemoveTruck(string truckId)
        {
            if (_activeTruck.Contains(truckId))
            {
                _activeTruck.TryTake(out truckId);
            }
        }

        public IReadOnlyCollection<string> GetAllTrucks()
        {
            return _activeTruck.ToList();
        }

        private void SetActiveTruckList()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITuckTrackingContext>();
            var truckTrackers = context.TruckTrackers
                .Where(tt => tt.Status == TruckStatus.Active)   
                .Select(tt => tt.TruckId.ToString())
                .ToList();

            _activeTruck.Clear();

            foreach (var truck in truckTrackers)
            {
                _activeTruck.Add(truck);
            }
        }

        public void Dispose()
        {
            _refreshTimer?.Dispose();
        }
    }
}
