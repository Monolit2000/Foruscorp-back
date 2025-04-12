using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Aplication.Contruct
{
    public interface IFuelRouteContext : IDisposable
    {
         DbSet<FuelRoute> FuelRoutes { get; set; }
         Task SaveChangesAsync(CancellationToken cancellationToken);
    }   
}
