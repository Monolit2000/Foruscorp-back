using MediatR;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Aplication.Contruct;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.GetCurrentFuel
{
    public record GetCurrentFuelDto(Guid TruckId, double CurrentFuelAmount);
    public record GetCurrentFuelCommand(Guid TruckId) : IRequest<GetCurrentFuelDto>;
    public class GetCurrentFuelCommandHandler(
        ITruckTrackingContext truckTrackingContext) : IRequestHandler<GetCurrentFuelCommand, GetCurrentFuelDto>
    {
        public async Task<GetCurrentFuelDto> Handle(GetCurrentFuelCommand request, CancellationToken cancellationToken)
        {
            var truckTracker = await truckTrackingContext.TruckTrackers
                .FirstOrDefaultAsync(x => x.TruckId == request.TruckId);

            if (truckTracker == null)
                throw new ArgumentException($"Truck with ID {request.TruckId} not found.", nameof(request.TruckId));

            var fuelPercentage = new GetCurrentFuelDto(truckTracker.TruckId, truckTracker.FuelStatus);

            return fuelPercentage;
        }
    }
}
