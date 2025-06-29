using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Domain.Trucks
{
    public class Route
    {
        public Guid RouteId { get; set; }
        public Guid TruckId { get; set; }
        public Guid TruckTrackerId { get; set; }
        //public List<TruckLocation> TruckLocations { get; set; } = [];

        public Guid Id { get; set; }


        public Route()
        {
                
        }

        public Route(Guid routeId, Guid truckTrackerId, Guid truckId)
        {
            RouteId = routeId;
            TruckTrackerId = truckTrackerId;
            TruckId = truckId;
        }   

        public static Route CreateNew(Guid routeId, Guid truckTrackerId, Guid truckId)
        {
            return new Route(routeId, truckTrackerId, truckId);
        }   
    }
}
