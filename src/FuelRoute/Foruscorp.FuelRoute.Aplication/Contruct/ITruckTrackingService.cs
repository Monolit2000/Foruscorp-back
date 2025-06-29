using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.Contruct
{
    public interface ITruckTrackingService
    {
        //Task<TrackedRouteDto> GetRouteAsync(Guid truckId);
        Task<TrackedRouteDto> GetRouteAsync(Guid truckId, CancellationToken cancellationToken = default);
    }
}
