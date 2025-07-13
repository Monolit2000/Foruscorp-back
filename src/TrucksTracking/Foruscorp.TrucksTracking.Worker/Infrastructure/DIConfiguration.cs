using MassTransit;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Worker.Infrastructure.Database;

namespace Foruscorp.TrucksTracking.Worker.Infrastructure
{
    public static class DIConfiguration
    {
        public static IServiceCollection AddTrucksTrackingWorkerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TruckTrackerWorkerContext>((sp, options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
            });

            //services.AddScoped<ITuckTrackingContext, TuckTrackingContext>();


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


            //services.AddMemoryCache();
            //services.AddSingleton<ActiveTruckManager, ActiveTruckManager>();
            //services.AddSingleton<TruckInfoManager>();
            //services.AddSingleton<ITruckProviderService, TruckProviderService>();

            return services;
        }
    }
}
