﻿using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Foruscorp.TrucksTracking.API.Realtime;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
using Foruscorp.TrucksTracking.Aplication.Contruct;


namespace Foruscorp.TrucksTracking.Processing.Services
{
    public class TruckTrackerProcessor(
     //IServiceScopeFactory serviceScopeFactory,
     IHubContext<TruckHub, ITruckLocationUpdateClient> hubContext,
     ILogger<TruckTrackerProcessor> logger,
     ActiveTruckManager activeTruckManager) : BackgroundService
    {

        private readonly Random _random = new();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateTruckLocation();
                await Task.Delay(1000, stoppingToken);
            }
        }


        private async Task UpdateTruckLocation()
        {
            foreach (var truck in activeTruckManager.GetAllTrucks())
            {
                //faker 
                var newLocation = CalculateNewLocation(new GeoPoint(12.23m, 123.32m));

                var update = new TruckLocationUpdate(truck, newLocation.Longitude, newLocation.Latitude);

                await hubContext.Clients.All.ReceiveTruckLocationUpdate(update);
                //await hubContext.Clients.Group(truck).ReceiveTruckLocationUpdate(update);

                logger.LogInformation("Updated {Tiker} location to Longitude: {newLocation.Longitude}, Longitude: {newLocation.Latitude}",
                    truck, newLocation.Longitude, newLocation.Latitude);
            }
        }

        private async Task UpdateTruckFuel()
        {
            foreach (var truck in activeTruckManager.GetAllTrucks())
            {
                //faker 
                var newLocation = CalculateNewLocation(new GeoPoint(12.23m, 123.32m));

                var update = new TruckLocationUpdate(truck, newLocation.Longitude, newLocation.Latitude);

                await hubContext.Clients.All.ReceiveTruckLocationUpdate(update);
                //await hubContext.Clients.Group(truck).ReceiveTruckLocationUpdate(update);

                logger.LogInformation("Updated {Tiker} location to Longitude: {newLocation.Longitude}, Longitude: {newLocation.Latitude}",
                    truck, newLocation.Longitude, newLocation.Latitude);
            }
        }


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
