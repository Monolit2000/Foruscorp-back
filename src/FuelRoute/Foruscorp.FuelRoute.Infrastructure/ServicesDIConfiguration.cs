using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.FuelRoutes.Infrastructure.Percistence;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using Foruscorp.FuelRoutes.Infrastructure.ApiClients;
using System.Runtime.Serialization;
using Microsoft.Extensions.Logging;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;
using Foruscorp.FuelRoutes.Infrastructure.Domain.FuelRoutes;


namespace Foruscorp.FuelRoutes.Infrastructure
{
    public static class ServicesDIConfiguration
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(IApplication).Assembly);
            });

            services.AddDbContext<FuelRouteContext>((sp, options) =>
            {
                //options.ReplaceService<IValueConverterSelector, StronglyTypedIdValueConverterSelector>();
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
                //options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            });

            services.AddMemoryCache();

            services.AddScoped<IFuelRouteContext, FuelRouteContext>();

            services.AddScoped<ITruckerPathApi, TruckerPathApiClient>();
            services.AddScoped<IFuelRouteRopository, FuelRouteRopository>();

            //services.AddScoped<TreatmentContext>();

            return services;
        }
    }
}
