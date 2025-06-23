using Foruscorp.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.SymbolStore;
using System.Linq;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public class FuelRoute : Entity, IAggregateRoot
    {
        public List<FuelRouteStation> FuelRouteStations = [];

        public readonly List<MapPoint> MapPoints = [];

        public readonly List<FuelRouteSection> RouteSections = [];

        public Guid Id { get; private set; }
        public Guid TruckId { get; private set; }

        public LocationPoint OriginLocation { get; private set; } 
        public LocationPoint DestinationLocation { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime ChangedAt { get; private set; }

        public bool IsAccepted { get; private set; }
        public byte[] RowVersion { get; set; }

        private FuelRoute() { } //For EF core 

        private FuelRoute(
            Guid truckId,
            LocationPoint originLocation,
            LocationPoint destinationLocation,
            List<FuelRouteStation> fuelPoints,
            List<MapPoint> mapPoints)
        {

            Id = Guid.NewGuid();
            TruckId = truckId;
            //DriverId = driverId;
            CreatedAt = DateTime.UtcNow;
            ChangedAt = DateTime.UtcNow;
            IsAccepted = true;

            if (fuelPoints.Any())
            {
                AddFuelPoints(fuelPoints);
            }
            if (mapPoints.Any())
            {
                AddMapPoints(mapPoints);
            }   
        }

        public static FuelRoute CreateNew(
            Guid truckId,
            //Guid driverId,
            LocationPoint originLocation,
            LocationPoint destinationLocation,
            List<FuelRouteStation> fuelPoints,
            List<MapPoint> mapPoints)
        {
            return new FuelRoute(
                truckId,
                //driverId,
                originLocation,
                destinationLocation,
                fuelPoints,
                mapPoints);
        }

        // Business methods

   


        public void SetRouteSections(IEnumerable<FuelRouteSection> sections)
        {
            if (sections == null || !sections.Any())
                throw new ArgumentException("Sections cannot be null or empty", nameof(sections));
            RouteSections.AddRange(sections);
            UpdateChangedAt();
        }   

        public void AddFuelPoint(FuelRouteStation fuelPoint)
        {
            if (fuelPoint == null)
                throw new ArgumentNullException(nameof(fuelPoint));

            ValidateFuelPoint(fuelPoint);
            FuelRouteStations.Add(fuelPoint);
            UpdateChangedAt();
        }

        //public void AddFuelPoints(IEnumerable<FuelRouteStations> fuelPoints)
        //{
        //    if (fuelPoints == null)
        //        throw new ArgumentNullException(nameof(fuelPoints));

        //    foreach (var point in fuelPoints)
        //    {
        //        if (FuelRouteStations.Any(existing => existing.FuelPointId == point.FuelPointId))
        //            continue; // пропускаємо дублікати

        //        ValidateFuelPoint(point);
        //        FuelRouteStations.Add(point);
        //    }

        //    UpdateChangedAt();
        //}

        public void AddFuelPoints(IEnumerable<FuelRouteStation> fuelPoints)
        {
            if (fuelPoints == null) throw new ArgumentNullException(nameof(fuelPoints));

            foreach (var point in fuelPoints)
            {
                if (FuelRouteStations.Any(x => x.FuelPointId == point.FuelPointId))
                    continue;

                FuelRouteStations.Add(point);
            }

            UpdateChangedAt();
        }

        public void AddMapPoints (IEnumerable<MapPoint> mapPoints)
        {
            if (mapPoints == null)
                throw new ArgumentNullException(nameof(mapPoints));

            MapPoints.AddRange(mapPoints);
            UpdateChangedAt();
        }



        public void RemoveFuelPoint(Guid fuelPointId)
        {
            var point = FuelRouteStations.FirstOrDefault(fp => fp.FuelPointId == fuelPointId);
            if (point == null)
                throw new InvalidOperationException($"Fuel point with ID {fuelPointId} not found");

            FuelRouteStations.Remove(point);
            UpdateChangedAt();
        }

        public void UpdateRouteDetails(string origin, string destination)
        {
            if (string.IsNullOrWhiteSpace(origin))
                throw new ArgumentException("Origin cannot be empty", nameof(origin));
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Destination cannot be empty", nameof(destination));
            if (origin == destination)
                throw new ArgumentException("Origin and destination cannot be the same");
       
            UpdateChangedAt();
        }

        public void ReassignDriver(Guid newDriverId)
        {
            if (newDriverId == Guid.Empty)
                throw new ArgumentException("Driver ID cannot be empty", nameof(newDriverId));

            //DriverId = newDriverId;
            UpdateChangedAt();
        }

        public void ReassignTruck(Guid newTruckId)
        {
            if (newTruckId == Guid.Empty)
                throw new ArgumentException("Truck ID cannot be empty", nameof(newTruckId));

            TruckId = newTruckId;
            UpdateChangedAt();
        }

        // Validation rules
        private void ValidateInitialParameters(Guid truckId,/* Guid driverId,*/ string origin, string destination)
        {
            if (truckId == Guid.Empty)
                throw new ArgumentException("Truck ID cannot be empty", nameof(truckId));
            //if (driverId == Guid.Empty)
            //    throw new ArgumentException("Driver ID cannot be empty", nameof(driverId));
            if (string.IsNullOrWhiteSpace(origin))
                throw new ArgumentException("Origin cannot be empty", nameof(origin));
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentException("Destination cannot be empty", nameof(destination));
            if (origin == destination)
                throw new ArgumentException("Origin and destination cannot be the same");
        }

        private void ValidateFuelPoint(FuelRouteStation fuelPoint)
        {
            //if (_fuelPoints.Any(fp => fp.Id == fuelPoint.Id))
            //    throw new InvalidOperationException("Duplicate fuel point ID");

            // Additional fuel point validation logic could be added here
        }

        private void UpdateChangedAt()
        {
            ChangedAt = DateTime.UtcNow;
        }
    }


    public static class GeoCalculator
    {
        public static bool IsPointWithinRadius(
            GeoPoint center,
            GeoPoint point,
            decimal radiusKm)
        {
            if (radiusKm <= 0)
                throw new ArgumentException("Radius must be positive", nameof(radiusKm));

            return CalculateHaversineDistance(center, point) <= radiusKm;
        }

        private static decimal CalculateHaversineDistance(GeoPoint point1, GeoPoint point2)
        {
            const decimal EarthRadiusKm = 6371;

            var dLat = ToRadians(point2.Latitude - point1.Latitude);
            var dLon = ToRadians(point2.Longitude - point1.Longitude);

            var a = (decimal)Math.Sin((double)(dLat / 2)) * (decimal)Math.Sin((double)(dLat / 2)) +
                    (decimal)Math.Cos((double)ToRadians(point1.Latitude)) * (decimal)Math.Cos((double)ToRadians(point2.Latitude)) *
                    (decimal)Math.Sin((double)(dLon / 2)) * (decimal)Math.Sin((double)(dLon / 2));

            var c = 2 * (decimal)Math.Atan2(Math.Sqrt((double)a), Math.Sqrt(1 - (double)a));
            return EarthRadiusKm * c;
        }

        private static decimal ToRadians(double degrees)
        {
            return (decimal)(degrees * (double)Math.PI / 180);
        }
    }
}
