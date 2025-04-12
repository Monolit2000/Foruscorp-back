using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MassTransit.Configuration;
using MediatR;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute
{
    public class AcceptFuelRouteCommand : IRequest<Result>
    {
        public string Id { get; set; }  
        public string RouteSectionId { get; set; }   
    }
}
