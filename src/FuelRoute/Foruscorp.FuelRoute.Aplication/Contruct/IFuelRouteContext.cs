using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Aplication.Contruct
{
    public interface IFuelRouteContext : IDisposable
    {
         DbSet<FuelRoute> FuelRoutes { get; set; }
         DbSet<MapPoint> MapPoints { get; set; }
         //Task BulkInsertAsync<T>(T entity) where T : class;
         Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }   
}
