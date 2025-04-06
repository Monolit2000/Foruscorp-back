using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Foruscorp.Trucks.Aplication.Drivers.GetAllDrivers
{
    public class GetAllDriversQuery : IRequest<List<DriverDto>>
    {
    }
}
