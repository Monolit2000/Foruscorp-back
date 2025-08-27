using Foruscorp.BuildingBlocks.Domain;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public enum LocationPointType
    {
        Origin = 1,
        Destination = 2,
        Stop = 3
    }

    public class LocationPoint : Entity, IAggregateRoot
    {
        public Guid? RouteId { get; private set; } 
        public Guid Id { get; private set; }
        public string Name { get; private set; } 
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public LocationPointType Type { get; private set; }
        
        public List<FuelRouteSection> FuelRouteSections { get; set; } = [];

        public int RouteVersion { get; set; } = 0; 
        private LocationPoint() { } // For EF Core
        private LocationPoint(
            string name, 
            double latitude, 
            double longitude,
            LocationPointType type)
        {
            Id = Guid.NewGuid();
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
            Type = type;
        }
        public static LocationPoint CreateNew(
            string name, 
            double latitude,
            double longitude,
            LocationPointType type,
            string description = null)
        {
            return new LocationPoint(
                name,
                latitude,
                longitude,
                type);
        }
        public void UpdateLocation(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public void UpdateType(LocationPointType type)
        {
            Type = type;
        }

        public void AddFuelRouteSection(FuelRouteSection fuelRouteSection)
        {
            if (!FuelRouteSections.Any(frs => frs.Id == fuelRouteSection.Id))
            {
                FuelRouteSections.Add(fuelRouteSection);
            }
        }

        public void RemoveFuelRouteSection(FuelRouteSection fuelRouteSection)
        {
            FuelRouteSections.RemoveAll(frs => frs.Id == fuelRouteSection.Id);
        }

        public void ClearFuelRouteSections()
        {
            FuelRouteSections.Clear();
        }
    }
}
