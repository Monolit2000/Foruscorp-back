using Foruscorp.FuelStations.Domain.FuelStations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Foruscorp.FuelStations.Aplication.Contructs.Utilities
{
    public static class FuelStationDeduplicationHelper
    {
        public static List<FuelStation> RemoveDuplicatesByCoordinates(
            List<FuelStation> stations, 
            int coordinatePrecision = 6)
        {
            if (stations == null || !stations.Any())
                return new List<FuelStation>();

            return stations
                .GroupBy(s => new 
                { 
                    Latitude = Math.Round(s.Coordinates.Latitude, coordinatePrecision), 
                    Longitude = Math.Round(s.Coordinates.Longitude, coordinatePrecision) 
                })
                .Select(group => 
                {
                    return group
                        .OrderBy(s => 
                        {
                            if (int.TryParse(s.FuelStationProviderId, out int id))
                                return id;
                            return int.MaxValue; 
                        })
                        .ThenBy(s => s.CreatedAt) 
                        .First();
                })
                .ToList();
        }

        public static int CountDuplicatesByCoordinates(
            List<FuelStation> stations, 
            int coordinatePrecision = 6)
        {
            if (stations == null || !stations.Any())
                return 0;

            var groupedByCoordinates = stations
                .GroupBy(s => new 
                { 
                    Latitude = Math.Round(s.Coordinates.Latitude, coordinatePrecision), 
                    Longitude = Math.Round(s.Coordinates.Longitude, coordinatePrecision) 
                })
                .Where(group => group.Count() > 1);

            return groupedByCoordinates.Sum(group => group.Count() - 1);
        }
    }
}
