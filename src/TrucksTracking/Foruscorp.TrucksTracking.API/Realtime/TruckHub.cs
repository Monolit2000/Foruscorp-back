using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace Foruscorp.TrucksTracking.API.Realtime
{
    public interface ITruckLocationUpdateClient
    {
        Task ReceiveTruckLocationUpdate(TruckLocationUpdate truckLocationUpdate);
    }

    public sealed record TruckLocationUpdate(string TruckId, decimal Longitude, decimal Latitude);

    public sealed class TruckHub(ActiveTruckManager activeTruckManager) : Hub<ITruckLocationUpdateClient>
    {

        public async Task JoinTruckGroup(string truckId)
        {
            activeTruckManager.AddTruck(truckId);
            await Groups.AddToGroupAsync(Context.ConnectionId, truckId);
        }
 
    }
}
