using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Domain.Trucks
{
    public class Truck
    {
        public Guid TruckId { get; private set; }
        public Guid CurrentRouteId { get; private set; }
        public List<TruckLocation> TruckLocationHistory { get; private set; } = [];
        public List<TruckFuel> FuelHistory { get; private set; } = []; 
        public Guid Id { get; private set; }
        public decimal FuelStatus { get; private set; }
        public TruckLocation CurrentTruckLocation { get; set; }
        public TruckStatus Status { get; private set; }

        private Truck(Guid truckId)
        {
            Id = Guid.NewGuid();
            TruckId = truckId;
            Status = TruckStatus.Inactive;
        }

        public static Truck Create(Guid truckId)
            => new Truck(truckId);

        public void UpdateCurrentTruckLocation(GeoPoint geoPoint)
        {
            var newLocation = TruckLocation.CreateNew(this.TruckId, this.CurrentRouteId, geoPoint);

            if (CurrentTruckLocation != null)
                TruckLocationHistory.Add(CurrentTruckLocation);

            CurrentTruckLocation = newLocation;
        }

        public void UpdateFuelStatus(decimal newFuelStatus, GeoPoint location)
        {
            FuelHistory.Add(TruckFuel.CreateNew(
                this.TruckId,
                this.FuelStatus,
                newFuelStatus,
                location));

            FuelStatus = newFuelStatus;
        }

        public void ChangeRoute(Guid newRouteId)
        {
            CurrentRouteId = newRouteId;
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
