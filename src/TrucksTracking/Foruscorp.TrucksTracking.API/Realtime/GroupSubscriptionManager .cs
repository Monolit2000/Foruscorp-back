using System.Collections.Concurrent;

namespace Foruscorp.TrucksTracking.API.Realtime
{
    public class TruckGroupSubscriptionManager 
    {
        // mapping: connectionId → set of truckIds
        private readonly ConcurrentDictionary<string, HashSet<string>> _map
            = new(StringComparer.Ordinal);

        public bool TrySubscribe(string connectionId, string truckId)
        {
            // Получаем (или создаём) HashSet для данного connectionId
            var set = _map.GetOrAdd(connectionId, _ => new HashSet<string>());

            lock (set)
            {
                // Если ещё нет подписки — добавляем и возвращаем true
                if (set.Add(truckId))
                    return true;

                // Иначе — уже подписан
                return false;
            }
        }

        public bool TryUnsubscribe(string connectionId, string truckId)
        {
            if (_map.TryGetValue(connectionId, out var set))
            {
                lock (set)
                {
                    var removed = set.Remove(truckId);
                    // Если больше нет подписок — можно убрать и сам ключ
                    if (set.Count == 0)
                        _map.TryRemove(connectionId, out _);
                    return removed;
                }
            }
            return false;
        }

        public void RemoveConnection(string connectionId)
        {
            _map.TryRemove(connectionId, out _);
        }
    }
}
