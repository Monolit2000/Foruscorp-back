using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MassTransit;
using MassTransit.Mediator;
using MediatR;

namespace Foruscorp.Trucks.Aplication.Drivers.UpdateDriverContact
{
    public class UpdateDriverContactCommand : IRequest<Result<DriverDto>>
    {
        public Guid DriverId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string TelegramLink { get; set; }    
    }
}
