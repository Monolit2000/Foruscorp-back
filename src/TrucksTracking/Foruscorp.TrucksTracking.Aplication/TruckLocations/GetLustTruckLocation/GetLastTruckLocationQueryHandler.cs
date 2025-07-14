using Foruscorp.TrucksTracking.Aplication.Contruct;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckLocations.GetLustTruckLocation
{
    public record GetLastTruckLocationsQuery(Guid TruckId) : IRequest<List<TruckLocationDto>>;
    public class GetLastTruckLocationQueryHandler(
        ITruckTrackingContext _context) : IRequestHandler<GetLastTruckLocationsQuery, List<TruckLocationDto>>
    {
        public async Task<List<TruckLocationDto>> Handle(GetLastTruckLocationsQuery request, CancellationToken cancellationToken)
        {
            var lastThree = await _context.TruckLocations
                .Where(l => l.TruckId == request.TruckId)
                .OrderByDescending(l => l.RecordedAt)
                .Take(3)
                .Select(l => new TruckLocationDto
                {
                    Latitude = l.Location.Latitude,
                    Longitude = l.Location.Longitude,
                    RecordedAt = l.RecordedAt
                }).ToListAsync(cancellationToken);

            if(!lastThree.Any())
                return new List<TruckLocationDto>();

            return lastThree;
        }
    }
}
