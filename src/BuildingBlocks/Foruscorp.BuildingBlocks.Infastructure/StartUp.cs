using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.BuildingBlocks.Infastructure.EventBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Foruscorp.BuildingBlocks.Infastructure
{
    public static class StartUp
    {
        public static IServiceCollection AddBuildingBlocksService(
           this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHostedService<IntegrationEventProcessorJob>();

            return services;
        }
    }
}
