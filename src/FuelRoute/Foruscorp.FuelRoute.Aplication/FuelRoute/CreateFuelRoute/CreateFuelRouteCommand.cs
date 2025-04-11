using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route;
using MediatR;
using Microsoft.VisualBasic;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoute.CreateFuelRoute
{
    public class CreateFuelRouteCommand : IRequest<object>
    {
        public RoutePlanningRequest RoutePlanningRequest { get; set; }  
    }
}
