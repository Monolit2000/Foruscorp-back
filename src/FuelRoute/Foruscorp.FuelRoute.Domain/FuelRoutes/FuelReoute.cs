using System;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using Foruscorp.BuildingBlocks.Domain;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public class FuelRoute : Entity, IAggregateRoot
    {
        public readonly List<RouteFuelStation> FuelPoints = [];

        public readonly List<MapPoint> MapPoints = [];

        public readonly List<RouteSection> RouteSections = [];

        public Guid Id { get; private set; }
        public Guid TruckId { get; private set; }
        public string Origin { get; private set; }
        public string Destination { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime ChangedAt { get; private set; }

        public string EncodeRoute { get; private set; } // Base64 encoded route     
        public bool IsAccepted { get; private set; }    

        private FuelRoute() { } //For EF core 

        private FuelRoute(
            Guid truckId,
            //Guid driverId,
            string origin,
            string destination,
            List<RouteFuelStation> fuelPoints,
            List<MapPoint> mapPoints)
        {
            ValidateInitialParameters(truckId, /*driverId,*/ origin, destination);

            Id = Guid.NewGuid();
            TruckId = truckId;
            //DriverId = driverId;
            Origin = origin;
            Destination = destination;
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
            string origin,
            string destination,
            List<RouteFuelStation> fuelPoints,
            List<MapPoint> mapPoints)
        {
            return new FuelRoute(
                truckId,
                //driverId,
                origin,
                destination,
                fuelPoints,
                mapPoints);
        }

        // Business methods

        public void AddEncodedRoute(string encodedRoute)
        {
            EncodeRoute = encodedRoute; 
        }

        public void SetRouteSections(IEnumerable<RouteSection> sections)
        {
            if (sections == null || !sections.Any())
                throw new ArgumentException("Sections cannot be null or empty", nameof(sections));
            RouteSections.AddRange(sections);
            UpdateChangedAt();
        }   

        public void AddFuelPoint(RouteFuelStation fuelPoint)
        {
            if (fuelPoint == null)
                throw new ArgumentNullException(nameof(fuelPoint));

            ValidateFuelPoint(fuelPoint);
            FuelPoints.Add(fuelPoint);
            UpdateChangedAt();
        }

        public void AddFuelPoints(IEnumerable<RouteFuelStation> fuelPoints)
        {
            if (fuelPoints == null)
                throw new ArgumentNullException(nameof(fuelPoints));

            foreach (var point in fuelPoints)
            {
                ValidateFuelPoint(point);
                FuelPoints.Add(point);
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
            var point = FuelPoints.FirstOrDefault(fp => fp.FuelPointId == fuelPointId);
            if (point == null)
                throw new InvalidOperationException($"Fuel point with ID {fuelPointId} not found");

            FuelPoints.Remove(point);
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

            Origin = origin;
            Destination = destination;
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

        private void ValidateFuelPoint(RouteFuelStation fuelPoint)
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
