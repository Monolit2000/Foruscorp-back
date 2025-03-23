using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Domain.Trucks
{
    public class TruckLocation
    {
        public Guid Id { get; set; }    
        public Guid TruckId { get; set; }
        public Guid RouteId { get; set; }
        public GeoPoint Location { get; set; }  
        public DateTime RecordedAt { get; set; }

        private TruckLocation(
            Guid truckId,
            Guid RouteId,
            GeoPoint location)
        {
            Id = Guid.NewGuid();
            TruckId = truckId;
            Location = location;
            RecordedAt = DateTime.UtcNow;    
        }

        public static TruckLocation CreateNew(
            Guid truckId,
            Guid routeId,
            GeoPoint location)
        {
            return new TruckLocation(
                truckId,
                routeId,
                location);    
        }
    }
}
