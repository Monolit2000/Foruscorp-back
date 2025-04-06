using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Foruscorp.Trucks.Aplication.Drivers.CreateDriver
{
    public class CreateDriverCommand : IRequest<DriverDto>
    {
        public string Name { get; set; }
    }
}
