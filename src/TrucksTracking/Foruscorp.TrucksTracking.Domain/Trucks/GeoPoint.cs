using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Domain.Trucks
{
    public class GeoPoint
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        private GeoPoint() { }

        public GeoPoint(double latitude, double longitude)
        {
            //if (latitude < -90 || latitude > 90)
            //    throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));
            //if (longitude < -180 || longitude > 180)
            //    throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
