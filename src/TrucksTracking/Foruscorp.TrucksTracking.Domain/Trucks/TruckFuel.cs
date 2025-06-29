using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Domain.Trucks
{
    public class TruckFuel
    {
        public Guid Id { get; private set; }
        public Guid TruckTrackerId { get; private set; }
        public Guid TruckId { get; private set; }
        public Guid? TruckLocationId { get; private set; } 
        public TruckLocation TruckLocation { get; set; } 

        public double PreviousFuelLevel { get; private set; }
        public double NewFuelLevel { get; private set; }
        public DateTime RecordedAt { get; private set; }

        private TruckFuel() { } //For EF core   

        private TruckFuel(
            Guid truckId,
            double previousFuelLevel,
            double newFuelLevel,
            TruckLocation truckLocation)
        {
            //Id = Guid.NewGuid();
            TruckId = truckId;
            PreviousFuelLevel = previousFuelLevel;
            NewFuelLevel = newFuelLevel;
            RecordedAt = DateTime.UtcNow;
            TruckLocation = truckLocation;

            TruckLocation = truckLocation;
            TruckLocationId = truckLocation?.Id;
        }

        public static TruckFuel CreateNew(
            Guid truckId,
            double previousFuelLevel,
            double newFuelLevel,
            TruckLocation truckLocation)
        {
            return new TruckFuel(
                truckId,
                previousFuelLevel,
                newFuelLevel,
                truckLocation);
        }
    }
}
