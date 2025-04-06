using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foruscorp.BuildingBlocks.Infastructure.MassTransitRabbitMqEventBus
{
    public static class MassTransitConfiguration
    {
        public static IServiceCollection AddMassTransitRabbitMqEventBus(
           this IServiceCollection services, IConfiguration configuration)
        {

            services.AddMassTransit(busConfiguration =>
            {
                busConfiguration.SetKebabCaseEndpointNameFormatter();

                busConfiguration.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(new Uri(configuration["MessageBroker:HostName"]!), h =>
                    {
                        h.Username(configuration["MessageBroker:Username"]!);
                        h.Username(configuration["MessageBroker:Password"]!);
                    });

                    configurator.ConfigureEndpoints(context);
                });
            });

            return services;
        }
    }
}
