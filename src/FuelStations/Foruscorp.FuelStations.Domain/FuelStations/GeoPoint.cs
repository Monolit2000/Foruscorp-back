using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Domain.FuelStations
{
    public class GeoPoint
    {
        [NotMapped]
        public double Latitude => Coordinates?.Y ?? 0.0;

        [NotMapped]
        public double Longitude => Coordinates?.X ?? 0.0;
        public Point Coordinates { get; private set; }

        private GeoPoint() { }

        public GeoPoint(double latitude, double longitude)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));
            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

            Coordinates = new Point(longitude, latitude) { SRID = 4326 };

            //Latitude = latitude;
            //Longitude = longitude;
        }
    }
}
