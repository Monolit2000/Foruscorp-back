using Microsoft.EntityFrameworkCore;    
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Foruscorp.Trucks.Infrastructure.Persistence;
using Foruscorp.Trucks.Aplication.Contruct;
using MassTransit;
using Foruscorp.Trucks.Infrastructure.ApiClients.SnsaraClient;

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

                busConfiguration.AddConsumers(typeof(IApplication).Assembly);
            });

            services.AddScoped<ITuckContext, TuckContext>();

            services.AddScoped<ITruckProviderService, SamsaraApiService>();

            return services;
        }
    }
}
