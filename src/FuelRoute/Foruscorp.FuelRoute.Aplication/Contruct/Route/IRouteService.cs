using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.Contruct.Route
{
    public interface IRouteService
    {
        public char[] GetRoute(string start, string end, string fuelType, int maxDistance, int maxTime, int maxCost, bool isReturnTrip, bool isFastestRoute, bool isCheapestRoute);
    }
}
