using Foruscorp.TrucksTracking.IntegrationEvents;
using MassTransit;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.FuelStations
{
    public record CheckNearFuelStationCommandTest(Guid truckId, Guid fuelStationId, string address, double longitude, double latitude, double distanceKm) : IRequest;
    public class CheckNearFuelStationCommandHandlerTest(IPublishEndpoint publishEndpoint) : IRequestHandler<CheckNearFuelStationCommandTest>
    {
        public async Task Handle(CheckNearFuelStationCommandTest request, CancellationToken cancellationToken)
        {
            await publishEndpoint.Publish(new FuelStationPlanMarkedAsNearIntegretionEvent(request.truckId, request.fuelStationId, request.address, request.longitude, request.latitude, request.distanceKm));
        }
    }
}
