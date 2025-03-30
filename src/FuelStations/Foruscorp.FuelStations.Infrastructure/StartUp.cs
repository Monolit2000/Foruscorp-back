using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Infrastructure.Percistence;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using Foruscorp.FuelStations.Infrastructure.WebScrapers;

namespace Foruscorp.FuelStations.Infrastructure
{
    public static class StartUp
    {
        public static IServiceCollection AddFuelStationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FuelStationContext>((sp, options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });


            services.AddScoped<IFuelStationsService, FuelStationsService>();

            services.AddScoped<IFuelStationContext, FuelStationContext>();


            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(IApplication).Assembly);
            });

            return services;
        }
    }
}
