using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;

namespace Foruscorp.Trucks.Aplication.Trucks.LoadTrucks
{
    public class LoadTrucksCommand : IRequest<Result<List<TruckDto>>>
    {
    }
}
