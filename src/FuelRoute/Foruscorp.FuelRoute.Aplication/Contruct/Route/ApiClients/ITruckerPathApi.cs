using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;

namespace Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients
{
    public interface ITruckerPathApi
    {
        public Task<DataObject> PlanRouteAsync();
    }
}
