using Foruscorp.TrucksTracking.Aplication.Contruct.RealTime;
using Foruscorp.TrucksTracking.Aplication.Contruct.RealTimeTruckModels;
using Microsoft.AspNetCore.SignalR;

namespace Foruscorp.TrucksTracking.API.Realtime
{
    public class SignalRNotificationSender(
        IHubContext<TruckHub, ITruckLocationUpdateClient> hubContext) : ISignalRNotificationSender
    {
        public async Task SendTruckLocationUpdateAsync(TruckLocationUpdate truckLocationUpdate)
        {
            await hubContext.Clients.Group(truckLocationUpdate.TruckId)
                .ReceiveTruckLocationUpdate(truckLocationUpdate);
        }
        public async Task SendTruckFuelUpdateAsync(TruckFuelUpdate truckFuelUpdate)
        {
            await hubContext.Clients.Group(truckFuelUpdate.TruckId)
                .ReceiveTruckFuelUpdate(truckFuelUpdate);
        }
        public async Task SendTruckStatusUpdateAsync(TruckStausUpdate truckStausUpdate)
        {
            await hubContext.Clients.Group(truckStausUpdate.TruckId)
                .ReceiveTruckStatusUpdate(truckStausUpdate);
        }
    }
}
