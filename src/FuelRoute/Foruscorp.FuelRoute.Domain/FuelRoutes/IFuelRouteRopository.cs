using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Domain.FuelRoutes
{
    public interface IFuelRouteRopository
    {
        Task BulkInsertAsync<T>(IEnumerable<T> entities) where T : class;
    }
}
