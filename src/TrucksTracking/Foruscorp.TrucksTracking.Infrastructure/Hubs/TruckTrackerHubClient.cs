using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
using Microsoft.AspNetCore.SignalR;

namespace Foruscorp.TrucksTracking.Infrastructure.Hubs
{
    internal class TruckTrackerHubClient
    {
    }

    //public interface ITruckLocationUpdateClient
    //{
    //    Task ReceiveTruckLocationUpdate(TruckLocationUpdate truckLocationUpdate);
    //}

    //public sealed record TruckLocationUpdate(string TruckId, decimal Longitude, decimal Latitude);

    //public sealed class TruckHub(ActiveTruckManager activeTruckManager) : Hub<ITruckLocationUpdateClient>
    //{

    //    public async Task JoinTruckGroup(string truckId)
    //    {
    //        activeTruckManager.AddTruck(truckId);
    //        await Groups.AddToGroupAsync(Context.ConnectionId, truckId);
    //    }

    //}
}
