using MediatR;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.AcceptFuelRoute
{
    public class AcceptFuelRouteCommand : IRequest<Result>
    {
        public Guid RourteId { get; set; }
    }
}
