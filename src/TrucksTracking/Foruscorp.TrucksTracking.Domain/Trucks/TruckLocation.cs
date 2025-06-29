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
        public Guid TruckId { get; set; }
        public Guid TruckTrackerId { get; set; }
        public Guid Id { get; set; }    
        public GeoPoint Location { get; set; }  
        public string FormattedLocation { get; set; }
        public DateTime RecordedAt { get; set; }

        public TruckLocation() { }

        private TruckLocation(
            Guid truckId,
            Guid truckTrackerId,
            GeoPoint location,
            string formattedLocation)
        {
            //Id = Guid.NewGuid();
            TruckId = truckId;
            TruckTrackerId = truckTrackerId;
            Location = location;
            RecordedAt = DateTime.UtcNow;
            FormattedLocation = formattedLocation;
        }

        public static TruckLocation CreateNew(
            Guid truckId,
            Guid truckTrackerId,
            Guid routeId,
            GeoPoint location,
            string formattedLocation)
        {
            return new TruckLocation(
                truckId,
                truckTrackerId,
                location,
                formattedLocation);    
        }
    }
}
