using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public class FuelRouteSection
    {
        public Guid Id { get; private set; }
        public Guid RouteId { get; private set; }
        public string EncodeRoute { get; private set; } // Base64 encoded route
        public RouteSectionInfo RouteSectionInfo { get; private set; }
        public bool IsEdited { get; set; } = false; 
        public int RouteVersion { get; set; } = 0; 
        public decimal FuelNeeded { get; set; } = 0.0m;
        public bool IsAccepted { get; private set; } = false;
        public bool IsAssigned { get; private set; } 

        public DateTime? AssignedAt { get; set; } 
        public DateTime? AcceptedAt { get; set; } 

        public List<FuelRouteStation> FuelRouteStations { get; set; } = [];
        public List<LocationPoint> LocationPoints { get; set; } = [];

        [NotMapped]
        public string RouteSectionResponceId { get; set; } 

        public FuelRouteSection() { } 

        public FuelRouteSection(Guid routeId, string encodeRoute)
        {
            Id = Guid.NewGuid();
            RouteId = routeId; 
            EncodeRoute = encodeRoute;
            IsAssigned = false; 
        }

        public FuelRouteSection(Guid routeId, string routeSectionResponceId, string encodeRoute)
        {
            Id = Guid.NewGuid();
            RouteId = routeId;
            EncodeRoute = encodeRoute;
            RouteSectionResponceId = routeSectionResponceId;
        }

        public RouteSectionInfo SetRouteSectionInfo(double Tolls, double Gallons, double Miles, int DriveTime)
        {
            var routeSectionInfo = new RouteSectionInfo(Tolls, Gallons, Miles, DriveTime);
            RouteSectionInfo = routeSectionInfo;
            return RouteSectionInfo;    
        }

        public void MarkAsAccepted()
        {
            IsAccepted = true;
            AcceptedAt = DateTime.UtcNow;
        }

        public void MarkAsAssigned()
        {
            IsAssigned = true;
            AssignedAt = DateTime.UtcNow;   
        }
        public void MarkAsUnassigned()
        {
            IsAssigned = false;
        }

        public void MarkAsEdited()
        {
            IsEdited = true;
            IsAssigned = false;
        }

        public void AddLocationPoint(LocationPoint locationPoint)
        {
            if (LocationPoints.Any(lp => lp.Id == locationPoint.Id))
                return;

            locationPoint.SetFuelRouteSection(Id);
            LocationPoints.Add(locationPoint);
        }

        public void RemoveLocationPoint(Guid locationPointId)
        {
            var locationPoint = LocationPoints.FirstOrDefault(lp => lp.Id == locationPointId);
            if (locationPoint != null)
            {
                locationPoint.RemoveFuelRouteSection();
                LocationPoints.Remove(locationPoint);
            }
        }

        public void ClearLocationPoints()
        {
            foreach (var locationPoint in LocationPoints)
            {
                locationPoint.RemoveFuelRouteSection();
            }
            LocationPoints.Clear();
        }

        public LocationPoint GetOriginLocation()
        {
            return LocationPoints.FirstOrDefault(lp => lp.Type == LocationPointType.Origin);
        }

        public LocationPoint GetDestinationLocation()
        {
            return LocationPoints.FirstOrDefault(lp => lp.Type == LocationPointType.Destination);
        }

        public IEnumerable<LocationPoint> GetStopLocations()
        {
            return LocationPoints.Where(lp => lp.Type == LocationPointType.Stop);
        }

        public void SetOriginLocation(LocationPoint originLocation)
        {
            // Удаляем старую точку отправления, если есть
            var existingOrigin = GetOriginLocation();
            if (existingOrigin != null)
            {
                RemoveLocationPoint(existingOrigin.Id);
            }

            // Устанавливаем новую точку как Origin
            originLocation.UpdateType(LocationPointType.Origin);
            AddLocationPoint(originLocation);
        }

        public void SetDestinationLocation(LocationPoint destinationLocation)
        {
            var existingDestination = GetDestinationLocation();
            if (existingDestination != null)
            {
                RemoveLocationPoint(existingDestination.Id);
            }

            destinationLocation.UpdateType(LocationPointType.Destination);
            AddLocationPoint(destinationLocation);
        }
    }
}
