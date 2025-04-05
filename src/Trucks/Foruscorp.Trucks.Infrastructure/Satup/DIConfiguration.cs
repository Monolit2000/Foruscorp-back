using Microsoft.EntityFrameworkCore;    
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.Trucks.Infrastructure.Persistence;
using Foruscorp.Trucks.Aplication.Contruct;

namespace Foruscorp.Trucks.Infrastructure.Satup
{
    public static class DIConfiguration
    {
        public static IServiceCollection AddTrucksServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TuckContext>((sp, options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(IApplication).Assembly);
            });

            services.AddScoped<ITuckContext, TuckContext>();

            return services;
        }
    }
}
