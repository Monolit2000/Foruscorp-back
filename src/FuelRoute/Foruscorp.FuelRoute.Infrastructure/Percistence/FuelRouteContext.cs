using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Infrastructure.Percistence
{
    public class FuelRouteContext : DbContext
    {
        public DbSet<FuelRoute> fuelRoutes { get; set; } 

        public FuelRouteContext(DbContextOptions<FuelRouteContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("FuelRoute");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuelRouteContext).Assembly);
        }
    }
}
