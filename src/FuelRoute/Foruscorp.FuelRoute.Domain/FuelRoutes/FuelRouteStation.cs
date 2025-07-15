using MediatR.NotificationPublishers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public class FuelRouteStation
    {
        public Guid FuelRouteId { get; set; }

        public Guid FuelPointId { get; set; }

        public Guid FuelStationId { get; set; }

        public DateTime ScheduledTime { get; set; }

        public bool IsOld { get; set; } = false;
        public int RouteVersion { get; set; } = 0; 

        public decimal Price { get; set; } 
        public decimal Discount { get; set; }
        public decimal PriceAfterDiscount { get; set; }
        public string Latitude { get; set; } 
        public string Longitude { get; set; } 

        public string Name { get; set; } 

        public string Address { get; set; }

        public bool IsAlgorithm { get; set; }

        public string Refill { get; set; } 

        public int StopOrder { get; set; }

        public string NextDistanceKm { get; set; } 

        public Guid RoadSectionId { get; set; }

        public double CurrentFuel { get; set; } = 0.0; // Default value 

        public FuelRouteStation() 
        {
            FuelStationId = Guid.NewGuid();

        } // For EF Core 


        public void MurkAsOld()
        {
            IsOld = true;
        }

        //private FuelRouteStations(
        //    GeoPoint location, 
        //    Guid fuelRouteId, 
        //    decimal fuelPrice,
        //    DateTime scheduledTime)
        //{
        //    FuelPointId = Guid.NewGuid();
        //    FuelRouteId = fuelRouteId;
        //    FuelPrice = fuelPrice;
        //    ScheduledTime = scheduledTime;
        //}

        //public static FuelRouteStations CreateNew(
        //    GeoPoint location,
        //    Guid fuelRouteId,
        //    decimal fuelPrice,
        //    DateTime scheduledTime)
        //{
        //    if (fuelRouteId == Guid.Empty)
        //        throw new ArgumentException("Fuel route ID cannot be empty", nameof(fuelRouteId));
        //    if (fuelPrice <= 0)
        //        throw new ArgumentException("Fuel price must be positive", nameof(fuelPrice));

        //    return new FuelRouteStations(
        //        location,
        //        fuelRouteId,
        //        fuelPrice, 
        //        scheduledTime);
        //}

    }
}
