using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Foruscorp.Trucks.Infrastructure.Persistence
{
    public static class MigrationExtensions
    {
        public static void ApplyTuckTrackingMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using TuckContext workContext = scope.ServiceProvider.GetRequiredService<TuckContext>();

            workContext.Database.Migrate();
        }
    }
}
