using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Users
{
    public class CreateUserCommand : IRequest
    {
        public Guid UserId { get; set; }
    }
}
