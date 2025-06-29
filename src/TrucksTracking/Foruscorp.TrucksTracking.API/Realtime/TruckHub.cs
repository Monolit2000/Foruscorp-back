using Microsoft.AspNetCore.SignalR;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTime;

namespace Foruscorp.TrucksTracking.API.Realtime
{
 
    public sealed class TruckHub(
        ActiveTruckManager activeTruckManager,
        TruckGroupSubscriptionManager truckGroupSubscriptionManager) : Hub<ITruckLocationUpdateClient>
    {
        public async Task JoinTruckGroup(string truckId)
        {
            var connectionId = Context.ConnectionId;

            if (truckGroupSubscriptionManager.TrySubscribe(connectionId, truckId))
            {
                activeTruckManager.AddTruck(truckId);
                await Groups.AddToGroupAsync(Context.ConnectionId, truckId);
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            truckGroupSubscriptionManager.RemoveConnection(Context.ConnectionId);

            // Find the truckId associated with this connection
            var truckId = Context.ConnectionId;
            if (truckId != null)
            {
                activeTruckManager.RemoveTruck(truckId);
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, truckId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }


}
