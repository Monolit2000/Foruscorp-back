
namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public class MapPoint
    {
        public Guid RouteId { get; private set; } 
        public Guid Id { get; private set; }    

        public GeoPoint GeoPoint { get; private set; }

        private MapPoint() { }
      
        private MapPoint(
            Guid routeId, 
            List<double> latAndLong)
        {
            Id = Guid.NewGuid();
            RouteId = routeId;
            GeoPoint = new GeoPoint(latAndLong[0], latAndLong[1]);  
        }

        public static MapPoint CreateNew(
            Guid routeId,
            List<double> latAndLong) 
            => new MapPoint(
                routeId, 
                latAndLong);
    }
}
