using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.TrucksTracking.Aplication.Contruct;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers
{
    public class ActiveTruckManager 
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private readonly ConcurrentBag<string> _activeTruck = [];


        public ActiveTruckManager(IServiceScopeFactory scopeFactory)  
        {
            _scopeFactory = scopeFactory;

            InitActiveTruckListAsinc();
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

        private void InitActiveTruckListAsinc()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITuckTrackingContext>();
            var truckTrackers = context.TruckTrackers
                .Select(tt => tt.TruckId.ToString())
                .ToList();

            _activeTruck.Clear();

            foreach (var truck in truckTrackers)
            {
                _activeTruck.Add(truck);
            }

        }
    }
}
