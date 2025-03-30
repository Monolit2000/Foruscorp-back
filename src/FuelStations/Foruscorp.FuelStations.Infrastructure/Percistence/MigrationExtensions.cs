using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Foruscorp.FuelStations.Infrastructure.Percistence
{
    public static class MigrationExtensions
    {
        public static void ApplyFuelStationContextMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using FuelStationContext workContext = scope.ServiceProvider.GetRequiredService<FuelStationContext>();

            workContext.Database.Migrate();
        }
    }
}
