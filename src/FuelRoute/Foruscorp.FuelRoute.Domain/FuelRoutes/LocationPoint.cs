using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public class LocationPoint : Entity, IAggregateRoot
    {
        public Guid RouteId { get; private set; } 
        public Guid Id { get; private set; }
        public string Name { get; private set; } 
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        private LocationPoint() { } // For EF Core
        private LocationPoint(
            string name, 
            double latitude, 
            double longitude)
        {
            Id = Guid.NewGuid();
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }
        public static LocationPoint CreateNew(
            string name, 
            double latitude,
            double longitude, 
            string description = null)
        {
            return new LocationPoint(
                name,
                latitude,
                longitude);
        }
        public void UpdateLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
