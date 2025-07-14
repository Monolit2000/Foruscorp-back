using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.IntegrationEvents;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Trucks.NotifiDriverAboutNearStation
{
    public record NotifyDriverAboutNearStationCommand(Guid TruckId, Guid FuelStationId, double Longitude, double Latitude, double DistanceKm) : IRequest;
    public class NotifyDriverAboutNearStationCommandHandler(
        ITruckContext truckContext,
        IPublishEndpoint publishEndpoint,
        ILogger<NotifyDriverAboutNearStationCommandHandler> logger) : IRequestHandler<NotifyDriverAboutNearStationCommand>
    {
        public async Task Handle(NotifyDriverAboutNearStationCommand request, CancellationToken cancellationToken)
        {
            var driver = truckContext.Drivers
                .Where(t => t.TruckId == request.TruckId)
                .FirstOrDefault();

            if (driver == null)
            {
                logger.LogWarning($"Driver not found for TruckId: {request.TruckId}");
                return;            
            }

            await publishEndpoint.Publish(new DriverNearFuelStationIntegrationEvent(driver.UserId.Value, request.FuelStationId, request.DistanceKm));

        }
    }
}
