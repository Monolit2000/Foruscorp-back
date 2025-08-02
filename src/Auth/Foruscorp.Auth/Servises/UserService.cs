using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.DataBase;
using Foruscorp.Auth.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Auth.Servises
{
    public class UserService(
        UserContext userContext) : IUserService
    {
        public Task CreateUserAsync(string userName, string email, string phone, string password)
        {
            throw new NotImplementedException();
        }

        public Task DeleteUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserEmailAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserPhoneAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserActiveAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserAdminAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsUserAuthenticatedAsync(string userId, string password)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserAsync(string userId, string userName, string email, string phone)
        {
            throw new NotImplementedException();
        }

        public async Task SetUserRole(Guid userId, string roleName)
        {
            if (!Enum.TryParse<UserRoleType>(roleName, ignoreCase: true, out var parsedRole))
            {
                throw new ArgumentException($"Invalid role: {roleName}");
            }

            // Удалим старые роли (если разрешено только одна роль на пользователя)
            var existingRoles = await userContext.Roles
                .Where(r => r.UserId == userId)
                .ToListAsync();

            userContext.Roles.RemoveRange(existingRoles);

            // Добавим новую роль
            var newRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = parsedRole
            };

            await userContext.Roles.AddAsync(newRole);
            await userContext.SaveChangesAsync();
        }

        public Task UpdateUserLastLoginAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
