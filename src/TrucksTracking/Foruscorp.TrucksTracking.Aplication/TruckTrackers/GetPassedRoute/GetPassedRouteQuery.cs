using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.GetPassedRoute
{
    public class GetPassedRouteQuery : IRequest<RouteDto>
    {
        public Guid TruckId { get; set; }  
    }
}
