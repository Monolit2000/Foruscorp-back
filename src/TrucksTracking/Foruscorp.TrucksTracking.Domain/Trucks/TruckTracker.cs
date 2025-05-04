using System;
using System.Collections.Generic;
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

        public Guid Id { get; private set; }

        public string ProviderTruckId { get; private set; }
        public Guid CurrentRouteId { get; private set; }
        public TruckStatus Status { get; private set; }
        public decimal FuelStatus { get; private set; }
        public TruckLocation CurrentTruckLocation { get; set; }

        private TruckTracker(
            Guid truckId, 
            string providerTruckId)
        {
            Id = Guid.NewGuid();
            TruckId = truckId;
            ProviderTruckId = providerTruckId;
            Status = TruckStatus.Inactive;
        }

        public static TruckTracker Create(
            Guid truckId,
            string providerTruckId)
            => new TruckTracker(
                truckId,
                providerTruckId);



        public void UpdateTruck(GeoPoint geoPoint, decimal newFuelStatus)
        {
            UpdateCurrentTruckLocation(geoPoint);

            UpdateFuelStatus(newFuelStatus);
        }



        public void UpdateCurrentTruckLocation(GeoPoint geoPoint)
        {
            var newLocation = TruckLocation.CreateNew(this.TruckId, this.CurrentRouteId, geoPoint);

            if (CurrentTruckLocation != null)
                TruckLocationHistory.Add(CurrentTruckLocation);

            CurrentTruckLocation = newLocation;
        }

        public void UpdateFuelStatus(decimal newFuelStatus/*, GeoPoint location*/)
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
                this.CurrentTruckLocation.Location));

            FuelStatus = newFuelStatus;
        }

        public void ChangeRoute(Guid newRouteId)
        {
            CurrentRouteId = newRouteId;
        }

        public void ActivateTruckTrucker()
        {
            if (Status == TruckStatus.Active)
                return;

            Status = TruckStatus.Active;
        }

        public void DeactivateTruckTrucker()
        {
            if (Status == TruckStatus.Inactive)
                return;
            Status = TruckStatus.Inactive;
        }

        public void SetStatus(TruckStatus newStatus)
        {
            if (newStatus == Status)
                return;

            //if (newStatus == TruckStatus.Active && FuelStatus <= 0)
            //    throw new InvalidOperationException("Cannot activate truck with no fuel");

            Status = newStatus;
        }
    }
}
