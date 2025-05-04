using MediatR;
using Foruscorp.TrucksTracking.Aplication.Contruct;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.GetAllTruckTrackers
{
    public class GetAllTruckTrackersQueryHandler(
        ITuckTrackingContext tuckTrackingContext) : IRequestHandler<GetAllTruckTrackersQuery, List<TruckTrackerDto>>
    {
        public Task<List<TruckTrackerDto>> Handle(GetAllTruckTrackersQuery request, CancellationToken cancellationToken)
        {
            var truckTrackers = tuckTrackingContext.TruckTrackers
                .Select(tt => new TruckTrackerDto
                {
                    TrackerId = tt.Id,
                    TruckId = tt.TruckId,
                    ProviderTruckId = tt.ProviderTruckId,
                    Status = tt.Status.ToString()
                }).ToListAsync(cancellationToken);

            return truckTrackers;   
        }
    }
}
