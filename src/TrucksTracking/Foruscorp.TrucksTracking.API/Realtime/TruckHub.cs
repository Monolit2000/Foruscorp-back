using Microsoft.AspNetCore.SignalR;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers;

namespace Foruscorp.TrucksTracking.API.Realtime
{
    public interface ITruckLocationUpdateClient
    {
        Task ReceiveTruckLocationUpdate(TruckLocationUpdate truckLocationUpdate);
        Task ReceiveTruckFuelUpdate(TruckFuelUpdate truckLocationUpdate);
    }

    public sealed record TruckLocationUpdate(
        string TruckId, 
        string TruckName, 
        double Longitude, 
        double Latitude, 
        string Time, 
        double HeadingDegrees);
    public sealed record TruckFuelUpdate(
        string TruckId, 
        double fuelPercentage);

    public sealed class TruckHub(ActiveTruckManager activeTruckManager) : Hub<ITruckLocationUpdateClient>
    {
        public async Task JoinTruckGroup(string truckId)
        {
            activeTruckManager.AddTruck(truckId);
            await Groups.AddToGroupAsync(Context.ConnectionId, truckId);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
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
