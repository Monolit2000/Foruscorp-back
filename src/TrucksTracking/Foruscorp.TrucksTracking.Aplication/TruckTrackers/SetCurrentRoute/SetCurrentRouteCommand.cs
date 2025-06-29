using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.TrucksTracking.Aplication.TruckTrackers.SetCurrentRoute
{
    public class SetCurrentRouteCommand : IRequest
    {
        public Guid TruckId { get; set; }
        public Guid RouteId { get; set; }
    }
}
