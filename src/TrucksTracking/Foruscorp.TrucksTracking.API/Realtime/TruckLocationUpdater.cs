using Microsoft.AspNetCore.SignalR;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Aplication.Contruct.TruckProvider;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Foruscorp.TrucksTracking.API.Realtime
{
    internal sealed class TruckLocationUpdater(
        //IServiceScopeFactory serviceScopeFactory,
        IHubContext<TruckHub, ITruckLocationUpdateClient> hubContext,
        ILogger<TruckLocationUpdater> logger,
        ActiveTruckManager activeTruckManager,
        IServiceScopeFactory scopeFactory,
        ITruckProviderService truckProviderService) : BackgroundService
    {

        private readonly Random _random = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                await UpdateTruckLocation();
                //await UpdateTruckFuel();
                await Task.Delay(1500, stoppingToken);
            }
        }


        private async Task UpdateTruckLocation()
        {
            var trucks = await UpdateTrucksStat();

            if (!trucks.Any())
                return;

            foreach (var truck in trucks)
            {
                var update = new TruckLocationUpdate(
                    truck.Name, 
                    truck.Gps.FirstOrDefault().Longitude, 
                    truck.Gps.FirstOrDefault().Latitude);

                await hubContext.Clients.All.ReceiveTruckLocationUpdate(update);
                await hubContext.Clients.Group(truck.Id.ToString()).ReceiveTruckLocationUpdate(update);

                logger.LogInformation("Updated {Tiker} location to Longitude: {newLocation.Longitude}, Longitude: {newLocation.Latitude}",
                    truck, truck.Gps.FirstOrDefault().Longitude, truck.Gps.FirstOrDefault().Latitude);
            }
        }



        private async Task<List<VehicleStat>> UpdateTrucksStat()
        {
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ITuckTrackingContext>();

            var trackers = await context.TruckTrackers
                .AsNoTracking()
                .Where(t => activeTruckManager.GetAllTrucks().Contains(t.TruckId.ToString()))
                .Select(t => t.ProviderTruckId)
                .ToListAsync();

            if (!trackers.Any())
                return new List<VehicleStat>();

            var responce = await truckProviderService.GetVehicleStatsFeedAsync(new List<string>());
                //trackers.Select(tt => tt.ProviderTruckId)
                //.ToList()

            responce.Data
                .Where(vs => trackers.Contains(vs.Id) || vs.EngineStates?.Any(es => es.Value == "On") == true)
                .ToList();  

            return responce.Data.ToList();    
        }

        //private async Task UpdateTruckFuel()
        //{
        //    foreach (var truck in activeTruckManager.GetAllTrucks())
        //    {
        //        //faker 
        //        var newLocation = CalculateNewLocation(new GeoPoint(12.23m, 123.32m));

        //        var update = new TruckLocationUpdate(truck, newLocation.Longitude, newLocation.Latitude);

        //        await hubContext.Clients.All.ReceiveTruckLocationUpdate(update);
        //        //await hubContext.Clients.Group(truck).ReceiveTruckLocationUpdate(update);

        //        logger.LogInformation("Updated {Tiker} location to Longitude: {newLocation.Longitude}, Longitude: {newLocation.Latitude}",
        //            truck, newLocation.Longitude, newLocation.Latitude);
        //    }
        //}


        private GeoPoint CalculateNewLocation(GeoPoint currentLocation)
        {
            const decimal MaxCoordinateChange = 0.01m;
            var random = _random;

            double change = (double)MaxCoordinateChange;
            decimal latFactor = (decimal)(random.NextDouble() * change * 2 - change);
            decimal lonFactor = (decimal)(random.NextDouble() * change * 2 - change);

            decimal latChange = currentLocation.Latitude * latFactor;
            decimal lonChange = currentLocation.Longitude * lonFactor;

            decimal newLatitude = Math.Max(-90m, Math.Min(90m, currentLocation.Latitude + latChange));
            decimal newLongitude = Math.Max(-180m, Math.Min(180m, currentLocation.Longitude + lonChange));

            newLatitude = Math.Round(newLatitude, 6);
            newLongitude = Math.Round(newLongitude, 6);

            return new GeoPoint(newLatitude, newLongitude);
        }

    }




    internal sealed class TruckLocationUpdateOptions
    {
        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(1);
    }
}
