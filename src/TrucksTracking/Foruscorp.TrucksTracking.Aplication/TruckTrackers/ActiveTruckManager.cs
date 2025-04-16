using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers
{
    public class ActiveTruckManager
    {
        private readonly ConcurrentBag<string> _activeTruck = [/*"#111", "#222", "#334"*/];

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
    }
}
