using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Infrastructure.ApiClients.SnsaraClient;
using Foruscorp.Trucks.Infrastructure.Persistence;
using Foruscorp.Trucks.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;    
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace Foruscorp.Trucks.Infrastructure.Satup
{
    public static class DIConfiguration
    {
        public static IServiceCollection AddTrucksServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<TruckContext>((sp, options) =>
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
                    configurator.Host(new Uri("rabbitmq://rabbitmq"/*configuration["MessageBroker:HostName"]!*/), h =>
                    {
                        h.Username(configuration["MessageBroker:Username"]!);
                        h.Username(configuration["MessageBroker:Password"]!);
                    });

                    configurator.ConfigureEndpoints(context);

                });

                busConfiguration.AddConsumers(typeof(IApplication).Assembly);
            });

            services.AddScoped<ITruckContext, TruckContext>();

            services.AddScoped<ITruckTrackingService, TruckTrackingService>();

            services.AddHttpClient<ITruckTrackingService, TruckTrackingService>(client =>
            {
                client.BaseAddress = new Uri("http://foruscorp.truckstracking.api:5001");
            });

            services.AddScoped<IPdfTransactionService, PdfTransactionService>();

            services.AddScoped<ITruckProviderService, SamsaraApiService>();

            services.AddScoped<ICurrentUser, CurrentUser>();

            return services;
        }
    }
}
