using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Foruscorp.TrucksTracking.Domain.Trucks
{
    public class TruckTracker
    {
        public Guid TruckId { get; private set; }
        public List<TruckFuel> FuelHistory { get; private set; } = [];
        public List<TruckLocation> TruckLocationHistory { get; private set; } = [];
        public List<Route> Routes { get; set; } = [];

        //public Guid? CurrentRouteId { get; private set; }
        public Route CurrentRoute { get; private set; }    

        public Guid Id { get; private set; }

        public string ProviderTruckId { get; private set; }
        public TruckStatus TruckStatus { get; private set; }

        public TruckEngineStatus TruckEngineStates { get; private set; } 

        public TruckTrackerStatus TruckTrackerStatus { get; private set; }
        public double FuelStatus { get; private set; }
        public TruckLocation CurrentTruckLocation { get; set; }

        private TruckTracker(
            Guid truckId, 
            string providerTruckId)
        {
            Id = Guid.NewGuid();
            TruckId = truckId;
            ProviderTruckId = providerTruckId;
            TruckStatus = TruckStatus.Inactive;
            TruckTrackerStatus = TruckTrackerStatus.Active;
        }

        public static TruckTracker Create(
            Guid truckId,
            string providerTruckId)
            => new TruckTracker(
                truckId,
                providerTruckId);


        public Route SetCurrentRoute(Guid routeId)
        {
            var route = Route.CreateNew(routeId, this.Id, this.TruckId);   

            CurrentRoute = route;

            Routes.Add(route);

            return route;
        }

        public void UpdateTruckTracker(Guid truckId, string providerTruckId)
        {
            TruckId = truckId;
            ProviderTruckId = providerTruckId;
        }


        public void UpdateTruck(GeoPoint geoPoint, string formattedLocation, double newFuelStatus)
        {
            UpdateCurrentTruckLocation(geoPoint, formattedLocation);

            UpdateFuelStatus(newFuelStatus);
        }


        public void UpdateCurrentTruckLocation(GeoPoint geoPoint, string formattedLocation)
        {
            var newLocation = TruckLocation.CreateNew(this.TruckId, this.Id, this.CurrentRoute?.TruckId, geoPoint, formattedLocation);

            if (CurrentTruckLocation != null)
                TruckLocationHistory.Add(newLocation);

            CurrentTruckLocation = newLocation;
        }

        public void UpdateFuelStatus(double newFuelStatus/*, GeoPoint location*/)
        {
            //FuelHistory.Add(TruckFuel.CreateNew(
            //    this.TruckId,
            //    this.FuelStatus,
            //    newFuelStatus,
            //    location));

            FuelHistory.Add(TruckFuel.CreateNew(
                this.TruckId,
                this.FuelStatus,
                newFuelStatus,
                this.CurrentTruckLocation));

            FuelStatus = newFuelStatus;
        }

        //public void ChangeRoute(Guid newRouteId)
        //{
        //    CurrentRouteId = newRouteId;
        //}

        public void ActivateTruckTrucker()
        {
            if (TruckStatus == TruckStatus.Active)
                return;

            TruckStatus = TruckStatus.Active;
        }

        public void DeactivateTruckTrucker()
        {
            if (TruckStatus == TruckStatus.Inactive)
                return;
            TruckStatus = TruckStatus.Inactive;
        }

        public void SetStatus(TruckStatus newStatus)
        {
            if (newStatus == TruckStatus)
                return;

            //if (newStatus == TruckStatus.Active && FuelStatus <= 0)
            //    throw new InvalidOperationException("Cannot activate truck with no fuel");

            TruckStatus = newStatus;
        }
    }
}
