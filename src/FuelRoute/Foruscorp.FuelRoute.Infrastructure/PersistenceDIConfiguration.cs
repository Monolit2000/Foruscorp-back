using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.FuelRoutes.Infrastructure.Percistence;
using Foruscorp.FuelRoutes.Aplication.Contruct;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using Foruscorp.FuelRoutes.Infrastructure.ApiClients;

namespace Foruscorp.FuelRoutes.Infrastructure
{
    public static class PersistenceDIConfiguration
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
                options.UseNpgsql(configuration.GetConnectionString("Database"));
                //options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            });

            services.AddScoped<ITruckerPathApi, TruckerPathApiClient>();


            //services.AddScoped<TreatmentContext>();

            return services;
        }
    }
}
