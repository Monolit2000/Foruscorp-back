using FluentResults;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.GetFuelRoute
{
    public class GetPussedFuelRouteQuery : IRequest<Result<RoutInfoDto>>
    {
        public Guid TruckId { get; set; }
    }
}
