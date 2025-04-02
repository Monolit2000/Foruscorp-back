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
        public Guid TruckId { get; private set; }
        public decimal PreviousFuelLevel { get; private set; }
        public decimal NewFuelLevel { get; private set; }
        public GeoPoint Location { get; private set; }
        public DateTime RecordedAt { get; private set; }

        private TruckFuel() { } //For EF core   

        private TruckFuel(
            Guid truckId,
            decimal previousFuelLevel,
            decimal newFuelLevel,
            GeoPoint location)
        {
            Id = Guid.NewGuid();
            TruckId = truckId;
            PreviousFuelLevel = previousFuelLevel;
            NewFuelLevel = newFuelLevel;
            Location = location;
            RecordedAt = DateTime.UtcNow;
        }

        public static TruckFuel CreateNew(
            Guid truckId,
            decimal previousFuelLevel,
            decimal newFuelLevel,
            GeoPoint location)
        {
            return new TruckFuel(
                truckId,
                previousFuelLevel,
                newFuelLevel,
                location);
        }
    }
}
