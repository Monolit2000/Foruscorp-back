using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.TrucksTracking.Infrastructure.Percistence;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Infrastructure.Percistence
{
    public class TuckTrackingContext : DbContext    
    {
        public TuckTrackingContext(DbContextOptions<TuckTrackingContext> options) : base(options)   
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("TuckTracking");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TuckTrackingContext).Assembly);
        }
    }
}
