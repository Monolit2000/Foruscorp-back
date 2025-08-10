using MediatR;
using System.Collections.Generic;

namespace Foruscorp.Trucks.Aplication.Users.GetAllUsers
{
    public record GetAllUsersQuery : IRequest<List<UserDto>>;
}
