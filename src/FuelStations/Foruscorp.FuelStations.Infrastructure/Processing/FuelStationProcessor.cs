using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Foruscorp.FuelStations.Aplication.FuelStations.LodadFuelStation;
using Microsoft.Extensions.DependencyInjection;

namespace Foruscorp.FuelStations.Infrastructure.Processing
{
    public class FuelStationProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<FuelStationProcessor> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                //await mediator.Send(new LodadFuelStationCommand());
                logger.LogInformation("FuelStationProcessor executed at: {time}", DateTimeOffset.Now);  
                await Task.Delay(TimeSpan.FromHours(2), stoppingToken);
            }
        }
    }
}
