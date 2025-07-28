using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.FuelStations.Aplication.Contructs;
using Foruscorp.FuelStations.Infrastructure.Percistence;
using Foruscorp.FuelStations.Aplication.Contructs.WebScrapers;
using Foruscorp.FuelStations.Infrastructure.WebScrapers;
using Foruscorp.FuelStations.Infrastructure.Processing;
using Microsoft.EntityFrameworkCore.Query;
using Foruscorp.FuelStations.Infrastructure.Services;

namespace Foruscorp.FuelStations.Infrastructure
{
    public static class StartUp
    {
        public static IServiceCollection AddFuelStationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<FuelStationContext>((sp, options) =>
            {
                options.UseNpgsql("Host=foruscorp.postgis.db;Port=5432;Database=ForuscorpDB-PostGIS;Username=postgres;Password=postgres" /*"Host=localhost;Port=5436;Database=ForuscorpDB-PostGIS;Username=postgres;Password=postgres"*/ /*configuration.GetConnectionString("DefaultConnection")*/);
                options.UseNpgsql(o => o.UseNetTopologySuite());
            });

            services.AddScoped<IFuelStationsService, FuelStationsService>();

            services.AddScoped<IFuelStationContext, FuelStationContext>();

            services.AddHostedService<FuelStationProcessor>();

            services.AddMemoryCache();

            services.AddScoped<ITruckProviderService, TruckProviderService>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(IApplication).Assembly);
            });

            return services;
        }
    }
}
