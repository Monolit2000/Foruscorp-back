using MediatR;
using System;

namespace Foruscorp.Trucks.Aplication.Users.GetUserById
{
    public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto>;
}
