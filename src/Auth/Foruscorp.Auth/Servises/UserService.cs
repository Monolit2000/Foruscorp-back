using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.DataBase;
using Foruscorp.Auth.Domain.Users;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Foruscorp.Auth.Servises
{
    public class UserService(
        UserContext userContext,
        ITokenProvider tokenProvider) : IUserService
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

        public async Task<string> SetCompanyId(Guid userId, Guid companyId)
        {
            var user = await userContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var company = await userContext.Companys
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if(company == null)
                throw new ArgumentException($"Company not exist");

            user.SetCompanyId(company.CompanyId);

            await userContext.SaveChangesAsync();

            var token = tokenProvider.Create(user);

            return token;
        }

        public async Task<string> SetUserRole(Guid userId, string roleName)
        {
            if (!Enum.TryParse<UserRoleType>(roleName, ignoreCase: true, out var parsedRole))
            {
                throw new ArgumentException($"Invalid role: {roleName}");
            }

            var existingRoles = await userContext.Roles
                .Where(r => r.UserId == userId)
                .ToListAsync();

            userContext.Roles.RemoveRange(existingRoles);

            var newRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = parsedRole
            };

            //await userContext.Roles.AddAsync(newRole);

            var user = await userContext.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            user.Roles.Add(newRole);

            await userContext.SaveChangesAsync();

            var token = tokenProvider.Create(user);

            return token;
        }

        public Task UpdateUserLastLoginAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }
}
