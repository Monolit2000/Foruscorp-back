using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.Configuration.GeoTools
{
    public static class GeoUtils
    {
        private const double EarthRadiusKm = 6371.0;

        public static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }


        public static List<List<double>> FilterPointsByDistance(List<List<double>> points, double targetDistanceKm = 15.0)
        {
            if (points == null || points.Count < 2)
                return points;

            var filteredPoints = new List<List<double>> { points[0] };
            double accumulatedDistance = 0.0;
            int lastSelectedIndex = 0;

            for (int i = 1; i < points.Count; i++)
            {
                var prevPoint = points[i - 1];
                var currPoint = points[i];
                double segmentDistance = CalculateDistance(
                    prevPoint[0], prevPoint[1],
                    currPoint[0], currPoint[1]
                );

                accumulatedDistance += segmentDistance;

                if (accumulatedDistance >= targetDistanceKm)
                {
                    filteredPoints.Add(currPoint);
                    accumulatedDistance = 0.0;
                    lastSelectedIndex = i;
                }
            }

            if (lastSelectedIndex < points.Count - 1)
            {
                filteredPoints.Add(points[points.Count - 1]);
            }

            return filteredPoints;
        }
    }
}
