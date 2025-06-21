using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public class FuelStopStation
    {
        public Guid FuelRouteId { get; private set; }

        public Guid FuelPointId { get; private set; }
        public GeoPoint Location { get; private set; }
        public decimal FuelPrice { get; private set; }
        public DateTime ScheduledTime { get; private set; }

        public string SectionId { get; private set; } 

        private FuelStopStation() { } // For EF Core 

        private FuelStopStation(
            GeoPoint location, 
            Guid fuelRouteId, 
            decimal fuelPrice,
            DateTime scheduledTime)
        {
            FuelPointId = Guid.NewGuid();
            FuelRouteId = fuelRouteId;
            Location = location;
            FuelPrice = fuelPrice;
            ScheduledTime = scheduledTime;
        }

        public static FuelStopStation CreateNew(
            GeoPoint location,
            Guid fuelRouteId,
            decimal fuelPrice,
            DateTime scheduledTime)
        {
            if (fuelRouteId == Guid.Empty)
                throw new ArgumentException("Fuel route ID cannot be empty", nameof(fuelRouteId));
            if (fuelPrice <= 0)
                throw new ArgumentException("Fuel price must be positive", nameof(fuelPrice));

            return new FuelStopStation(
                location,
                fuelRouteId,
                fuelPrice, 
                scheduledTime);
        }

    }
}
