using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.FuelRoutes.Infrastructure.Percistence;

namespace Foruscorp.FuelStations.Infrastructure.Percistence
{
    public static class MigrationExtensions
    {
        public static void ApplyFuelRouteContextMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using FuelRouteContext workContext = scope.ServiceProvider.GetRequiredService<FuelRouteContext>();

            workContext.Database.Migrate();
        }
    }
}
