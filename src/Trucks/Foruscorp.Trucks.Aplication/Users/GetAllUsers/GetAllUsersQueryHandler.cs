using Foruscorp.Trucks.Aplication.Contruct;
using Foruscorp.Trucks.Aplication.Users;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Trucks.Aplication.Users.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly ITruckContext _truckContext;

        public GetAllUsersQueryHandler(ITruckContext truckContext)
        {
            _truckContext = truckContext;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _truckContext.Users
                .Include(u => u.Contact)
                .AsNoTracking()
                .OrderBy(u => u.CreatedAt)
                .ToListAsync(cancellationToken);

            return users.Select(u => u.ToUserDto()).ToList();
        }
    }
}
