using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit.Configuration;

namespace Foruscorp.FuelRoutes.Aplication.Configuration.CaheKeys
{
    public static class FuelRoutesCachKeys
    {
        public static string RouteById(string id) => $"Route-{id}";
    }
}
