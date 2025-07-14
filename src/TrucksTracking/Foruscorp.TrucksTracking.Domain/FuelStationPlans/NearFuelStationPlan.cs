using Foruscorp.TrucksTracking.Domain.Trucks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Domain.FuelStationPlans
{
    public class NearFuelStationPlan
    {
        public Guid TruckId { get; private set; }   
        public Guid FuelStationId { get; private set; }
        public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public double Longitude { get; private set; }
        public double Latitude { get; private set; }
        public bool IsNear { get; set; } 
        public double NearDistance { get; private set; }
        public Guid? RecordedOnLocationId { get; set; }
        public TruckLocation RecordedOnLocation { get; set; }

        public NearFuelStationPlan() { }

        private NearFuelStationPlan(Guid fuelStationId, Guid truckId, double longitude, double latitude, double nearDistance = 10.0)
        {
            TruckId = truckId;
            FuelStationId = fuelStationId;
            Longitude = longitude;
            Latitude = latitude;
            IsNear = false;
            NearDistance = nearDistance;    
            CreatedAt = DateTime.UtcNow;
        }
        public static NearFuelStationPlan Create(Guid fuelStationId, Guid truckId, double longitude, double latitude, double nearDistance = 10.0)
            => new NearFuelStationPlan(fuelStationId, truckId, longitude, latitude, nearDistance);

        public void MarkIsNear(TruckLocation recordedOnLocation)
        {
            IsNear = true;
            RecordedOnLocation = recordedOnLocation;
        }
    }
}
