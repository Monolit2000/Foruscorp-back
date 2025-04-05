using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Foruscorp.Trucks.Aplication.Trucks.GetAllTruks
{
    public class GetAllTruksQuery : IRequest<List<TruckDto>>
    {
    }
}
