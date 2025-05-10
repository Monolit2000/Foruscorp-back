using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit.Configuration;
using MassTransit.DependencyInjection.Testing;
using MediatR;

namespace Foruscorp.Trucks.Aplication.Drivers.CreateDriver
{
    public class CreateDriverCommand : IRequest<DriverDto>
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }    
        public string TelegramLink { get; set; }    
    }
}
