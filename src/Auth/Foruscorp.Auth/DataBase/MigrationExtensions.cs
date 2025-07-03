using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Auth.DataBase
{
    public static class MigrationExtensions
    {
        public static void ApplyFuelRouteContextMigrations(this IApplicationBuilder app)
        {
            using IServiceScope scope = app.ApplicationServices.CreateScope();

            using UserContext workContext = scope.ServiceProvider.GetRequiredService<UserContext>();

            workContext.Database.Migrate();
        }
    }
}
