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
using MassTransit;
using Foruscorp.FuelRoutes.Infrastructure.Services;


namespace Foruscorp.FuelRoutes.Infrastructure
{
    public static class ServicesDIConfiguration
    {
        public static IServiceCollection AddPersistenceServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

            services
                .AddHttpClient<ITruckTrackingService, TruckTrackingService>(client =>
                {
                    client.BaseAddress = new Uri("http://foruscorp.truckstracking.api:5001");
                });

            services.AddScoped<ITruckTrackingService, TruckTrackingService>();

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

            services.AddMassTransit(busConfiguration =>
            {
                busConfiguration.SetKebabCaseEndpointNameFormatter();

                busConfiguration.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(new Uri("rabbitmq://rabbitmq"/*configuration["MessageBroker:HostName"]!*/), h =>
                    {
                        h.Username(configuration["MessageBroker:Username"]!);
                        h.Username(configuration["MessageBroker:Password"]!);
                    });

                    configurator.ConfigureEndpoints(context);

                });

                busConfiguration.AddConsumers(typeof(IApplication).Assembly);
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
