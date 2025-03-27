using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;        
using Microsoft.Extensions.DependencyInjection;

namespace Foruscorp.TrucksTracking.Infrastructure.Percistence
{
    public static class MigrationExtensions
    {
        public static void ApplyTuckTrackingMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using TuckTrackingContext workContext = scope.ServiceProvider.GetRequiredService<TuckTrackingContext>();

            workContext.Database.Migrate();
        }
    }
}
