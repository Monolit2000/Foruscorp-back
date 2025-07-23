using MediatR;
using Microsoft.AspNetCore.SignalR;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
using Foruscorp.TrucksTracking.Aplication.TruckLocations;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTime;
using Foruscorp.TrucksTracking.Aplication.TruckLocations.GetLustTruckLocation;

namespace Foruscorp.TrucksTracking.API.Realtime
{
 
    public sealed class TruckHub(
        ActiveTruckManager activeTruckManager,
        TruckGroupSubscriptionManager truckGroupSubscriptionManager,
        ISender sender) : Hub<ITruckLocationUpdateClient>
    {
        public async Task JoinTruckGroup(string truckId)
        {
            var result = new List<TruckLocationDto>();
            var connectionId = Context.ConnectionId;
         
            activeTruckManager.AddTruck(truckId);
            await Groups.AddToGroupAsync(connectionId, truckId);

            //if (Guid.TryParse(truckId, out var truckGuid))
            //{
            //    result = await sender.Send(
            //        new GetLastTruckLocationsQuery(truckGuid),
            //        CancellationToken.None);
            //}

            //return result;
        }

        public async Task LeaveTruckGroup(string truckId)
        {
            var connectionId = Context.ConnectionId;

            await Groups.RemoveFromGroupAsync(connectionId, truckId);
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
