using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Domain;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Infrastructure.Percistence;

namespace Foruscorp.FuelRoutes.Infrastructure.Domain.FuelRoutes
{
    public class FuelRouteRopository(FuelRouteContext context) : IFuelRouteRopository
    {

        public async Task BulkInsertAsync<T>(IEnumerable<T> entities ) where T : class
        {
            await context.BulkInsertAsync(entities);
        }

        public async Task AddAsync(FuelRoute fuelRoute)
        {
            await context.FuelRoutes.AddAsync(fuelRoute);
            await context.SaveChangesAsync();
        }
        public async Task<FuelRoute> GetByIdAsync(Guid id)
        {
            return await context.FuelRoutes.FindAsync(id);
        }
        public async Task<IEnumerable<FuelRoute>> GetAllAsync()
        {
            return await context.FuelRoutes.ToListAsync();
        }
        public async Task UpdateAsync(FuelRoute fuelRoute)
        {
            context.FuelRoutes.Update(fuelRoute);
            await context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var fuelRoute = await GetByIdAsync(id);
            if (fuelRoute != null)
            {
                context.FuelRoutes.Remove(fuelRoute);
                await context.SaveChangesAsync();
            }
        }   

    }
}
