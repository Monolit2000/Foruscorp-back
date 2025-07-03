using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Domain.Users;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Trucks.Aplication.Users
{
    public class CreateUserCommandHandler(
        ITruckContext context) : IRequestHandler<CreateUserCommand>
    {
        public async Task Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = User.CreateNew(request.UserId);

            await context.Users.AddAsync(user, cancellationToken);  

            await context.SaveChangesAsync(cancellationToken);  
        }
    }
}
