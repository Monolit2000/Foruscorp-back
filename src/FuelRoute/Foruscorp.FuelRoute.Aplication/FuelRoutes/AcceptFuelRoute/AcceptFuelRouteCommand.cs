using MediatR;
using FluentResults;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute
{
    public class AcceptFuelRouteCommand : IRequest<Result>
    {
        public string Id { get; set; }  
        public string RouteSectionId { get; set; }   
    }
}
