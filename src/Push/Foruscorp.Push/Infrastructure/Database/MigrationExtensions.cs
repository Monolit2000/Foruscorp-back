using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Infrastructure.Database
{
    public static class MigrationExtensions
    {
        public static void ApplyPushContextMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using PushNotificationsContext pushContext = scope.ServiceProvider.GetRequiredService<PushNotificationsContext>();

            pushContext.Database.Migrate();
        }
    }
}
