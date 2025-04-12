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
using Foruscorp.FuelRoutes.Domain.FuelRoutes;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.CreateFuelRoute
{
    public class CreateFuelRouteCommand : IRequest<object>
    {
        public GeoPoint Origin { get; set; }
        public GeoPoint Destinations { get; set; }
        //public RoutePlanningRequest RoutePlanningRequest { get; set; }  
    }
}
