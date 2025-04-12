using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients
{
    public interface ITruckerPathApi
    {
        public Task<DataObject> PlanRouteAsync(GeoPoint origin, GeoPoint destinations, CancellationToken cancellationToken = default);
    }
}
