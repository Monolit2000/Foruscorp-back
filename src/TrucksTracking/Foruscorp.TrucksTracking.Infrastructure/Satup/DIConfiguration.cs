using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.TrucksTracking.Infrastructure.Percistence;
using MassTransit;
using Foruscorp.TrucksTracking.Aplication;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Aplication.TruckTrackers;
using Foruscorp.TrucksTracking.Infrastructure.Services;

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

            services.AddScoped<ITruckTrackingContext, TuckTrackingContext>();


            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(IApplication).Assembly);
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
            services.AddSingleton<ActiveTruckManager, ActiveTruckManager>();
            services.AddSingleton<TruckInfoManager>();
            services.AddSingleton<ITruckProviderService, TruckProviderService>();

            return services;
        }
    }
}
