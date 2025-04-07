using System.Collections.Concurrent;

namespace Foruscorp.TrucksTracking.API.Realtime
{
    public sealed class ActiveTruckManager
    {
        private readonly ConcurrentBag<string> _activeTruck = ["#111", "#222", "#334"];

        public void AddTruck(string truckId)
        {
            if(!_activeTruck.Contains(truckId))
            {
                _activeTruck.Add(truckId);  
            }
        }

        public IReadOnlyCollection<string> GetAllTrucks()
        {
            return _activeTruck.ToList();
        }
            
    }
}
