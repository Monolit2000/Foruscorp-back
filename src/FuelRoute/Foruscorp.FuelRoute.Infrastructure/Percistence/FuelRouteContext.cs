using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Aplication.Contruct;

namespace Foruscorp.FuelRoutes.Infrastructure.Percistence
{
    public class FuelRouteContext : DbContext, IFuelRouteContext
    {
        public DbSet<FuelRoute> FuelRoutes { get; set; }

        public FuelRouteContext(DbContextOptions<FuelRouteContext> options) : base(options)
        {
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("FuelRoutes");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuelRouteContext).Assembly);
        }


    }
}
