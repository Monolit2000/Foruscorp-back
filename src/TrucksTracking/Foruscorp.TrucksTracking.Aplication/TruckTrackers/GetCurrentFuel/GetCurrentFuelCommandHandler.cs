using Foruscorp.TrucksTracking.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.GetCurrentFuel
{
    public record GetCurrentFuelDto(Guid TruckId, decimal CurrentFuelAmount);
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

            //var currentFuelAmount = truckTracker.CurrentFuelAmount;

            throw new NotImplementedException();
        }
    }
}
