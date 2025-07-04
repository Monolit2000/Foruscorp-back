﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Foruscorp.FuelRoutes.Infrastructure.Percistence
{
    public class FuelRouteContext : DbContext, IFuelRouteContext
    {
        public DbSet<FuelRoute> FuelRoutes { get; set; }

        public DbSet<MapPoint> MapPoints { get; set; }

        public DbSet<FuelRouteStation> FuelRouteStation { get; set; }

        public DbSet<FuelRouteSection> RouteSections { get; set; }

        public FuelRouteContext(DbContextOptions<FuelRouteContext> options) : base(options)
        {
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await base.SaveChangesAsync(cancellationToken);
        }

        //public async Task BulkInsertAsync<T>(T entity) where T : class
        //{
        //    await this.BulkInsertAsync(entity);
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("FuelRoutes");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FuelRouteContext).Assembly);
        }


    }
}
