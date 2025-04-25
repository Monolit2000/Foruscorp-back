using FluentResults;
using Foruscorp.FuelRoutes.Aplication.Contruct.Route.ApiClients;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelRoutes.Aplication.FuelRoutes.DropPiont
{
    public class DropPiontCommand : IRequest<Result<SimpleDropPointResponse>>
    {
        public double latitude { get; set; }
        public double longitude { get; set; }

        public int level = 4;
        public int radius = 10000;
    }
}
