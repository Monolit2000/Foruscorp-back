using Foruscorp.Push.Contruct;
using Foruscorp.Push.Infrastructure.Database;
using Foruscorp.Push.Infrastructure.Services;
using MassTransit;
using MassTransit.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Infrastructure
{
    public static class DIConfiguration
    {

        public static IServiceCollection AddPushInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<PushNotificationsContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));


            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(IApplication).Assembly);
            });

            services.AddScoped<IExpoPushService, ExpoPushService>();


            // Register MassTransit for message bus
            services.AddMassTransit(busConfiguration =>
            {
                busConfiguration.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(new Uri(configuration["MessageBroker:Host"]!), h => 
                    {
                        h.Username(configuration["MessageBroker:Username"]!);
                        h.Password(configuration["MessageBroker:Password"]!);
                    });

                    configurator.ConfigureEndpoints(context);
                });

                busConfiguration.AddConsumers(typeof(IApplication).Assembly);

            });
            return services;
        }

    }
}
