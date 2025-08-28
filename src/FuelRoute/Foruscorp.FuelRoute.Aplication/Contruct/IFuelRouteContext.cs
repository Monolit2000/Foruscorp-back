using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Domain.RouteValidators;

namespace Foruscorp.FuelRoutes.Aplication.Contruct
{
    public interface IFuelRouteContext : IDisposable
    {
         DbSet<FuelRoute> FuelRoutes { get; set; }
         DbSet<MapPoint> MapPoints { get; set; }

         DbSet<FuelRouteStation> FuelRouteStation { get; set; }

         DbSet<RouteValidator> RouteValidators { get; set; }    
         DbSet<FuelRouteSection> RouteSections { get; set; }
        //Task BulkInsertAsync<T>(T entity) where T : class;
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }   
}
