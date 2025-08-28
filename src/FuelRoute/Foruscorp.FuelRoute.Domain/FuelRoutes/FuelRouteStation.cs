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

        public bool IsAssigned { get; set; }
        public bool IsPlaned { get; set; }


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
        public double ForwardDistance { get; private set; } = 0.0; // Distance to next station or destination

        public FuelRouteStation() 
        {
            FuelStationId = Guid.NewGuid();

        } // For EF Core 


        public void MurkAsOld()
        {
            IsOld = true;
        }

        public void Assign()
        {
            IsPlaned = true;
            IsAssigned = true;
        }

        public void SetForwardDistance(double distance)
        {
            if (distance < 0)
                throw new ArgumentException("Forward distance cannot be negative", nameof(distance));
            
            ForwardDistance = distance;
        }

        /// <summary>
        /// Extracts route points from polyline data
        /// </summary>
        /// <param name="points">List of coordinate pairs</param>
        /// <returns>List of GeoPoint objects</returns>
        public List<GeoPoint> ExtractRoutePoints(List<List<double>> points)
        {
            var routePoints = points.Select(p => new GeoPoint(p[0], p[1]))
                .ToList();

            return routePoints;
        }

        /// <summary>
        /// Calculates distance using Haversine formula between two points
        /// </summary>
        /// <param name="lat1">Latitude of first point</param>
        /// <param name="lon1">Longitude of first point</param>
        /// <param name="lat2">Latitude of second point</param>
        /// <param name="lon2">Longitude of second point</param>
        /// <returns>Distance in kilometers</returns>
        public static double CalculateDistanceBetweenPoints(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return earthRadius * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

    

    }
}
