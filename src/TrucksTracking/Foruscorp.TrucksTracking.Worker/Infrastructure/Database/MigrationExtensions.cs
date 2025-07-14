using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Worker.Infrastructure.Database
{
    public static class MigrationExtensions
    {
        public static void ApplyTruckTrackerWorkerContextMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using TruckTrackerWorkerContext pushContext = scope.ServiceProvider.GetRequiredService<TruckTrackerWorkerContext>();

            pushContext.Database.Migrate();
        }
    }
}
