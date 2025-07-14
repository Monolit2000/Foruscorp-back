using MediatR;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Foruscorp.TrucksTracking.Domain.FuelStationPlans;

namespace Foruscorp.TrucksTracking.Aplication.FuelStations.PlanNearFuelStation
{
    public record PlanNearFuelStationCommand(Guid FuelStationId, Guid RouteId, Guid TruckId, double Longitude, double Latitude) : IRequest;
    public class PlanNearFuelStationCommandHandler(
        ITruckTrackingContext tuckTrackingContext) : IRequestHandler<PlanNearFuelStationCommand>
    {
        public async Task Handle(PlanNearFuelStationCommand request, CancellationToken cancellationToken)
        {
            var truckTracker = await tuckTrackingContext.TruckTrackers
                .FirstOrDefaultAsync(tt => tt.TruckId == request.TruckId);

            if (truckTracker == null)
                return;

            var fuelStationPlan = NearFuelStationPlan.Create(
                request.FuelStationId, truckTracker.TruckId, request.RouteId, request.Longitude, request.Latitude);

            await tuckTrackingContext.NearFuelStationPlans.AddAsync(fuelStationPlan, cancellationToken);

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);  
        }
    }
}
