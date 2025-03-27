using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.TrucksTracking.Infrastructure.Percistence;

namespace Foruscorp.TrucksTracking.Infrastructure.Satup
{
    public static class DIConfiguration
    {
        public static IServiceCollection AddTrucksTrackingServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TuckTrackingContext>((sp, options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            //services.AddMediatR(cfg =>
            //{
            //    cfg.RegisterServicesFromAssembly(typeof(IApplication).Assembly);
            //});

            return services;
        }
    }
}
