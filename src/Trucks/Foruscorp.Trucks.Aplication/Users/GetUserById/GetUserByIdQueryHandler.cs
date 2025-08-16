using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Aplication.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Users.GetUserById
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        private readonly ITruckContext _truckContext;

        public GetUserByIdQueryHandler(ITruckContext truckContext)
        {
            _truckContext = truckContext;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _truckContext.Users
                .Include(u => u.Contact)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, cancellationToken);

            return user?.ToUserDto();
        }
    }
}
