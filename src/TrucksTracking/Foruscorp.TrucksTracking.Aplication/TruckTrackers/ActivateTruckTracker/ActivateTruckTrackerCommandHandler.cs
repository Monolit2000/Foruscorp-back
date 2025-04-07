using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Foruscorp.TrucksTracking.Domain.Trucks;
using Foruscorp.TrucksTracking.Aplication.Contruct;


namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.ActivateTruckTracker
{
    public class ActivateTruckTrackerCommandHandler(
        ITuckTrackingContext tuckTrackingContext) : IRequestHandler<ActivateTruckTrackerCommand, Result<TruckTrackerDto>>
    {
        public async Task<Result<TruckTrackerDto>> Handle(ActivateTruckTrackerCommand request, CancellationToken cancellationToken)
        {
            var truckTracker = await tuckTrackingContext.TruckTrackers
                .FirstOrDefaultAsync(tt => tt.TruckId == request.TruckId);

            if (truckTracker == null)
                return Result.Fail("truckTracker not exist");

            truckTracker.SetStatus(TruckStatus.Active);

            await tuckTrackingContext.SaveChangesAsync(cancellationToken);

            return Result.Ok(new TruckTrackerDto
            {
                TrackerId = truckTracker.Id,
                TruckId = truckTracker.TruckId,
                Status = truckTracker.Status.ToString()
            }); 
        }
    }
}
