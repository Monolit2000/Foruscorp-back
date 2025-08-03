using FluentResults;
using Foruscorp.Auth.Contruct;
using Foruscorp.Auth.DataBase;
using Foruscorp.Auth.Domain.Users;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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


        public async Task<Result<UserDto>> GetUserByNameAsync(string name)
        {
            var user = await userContext.Users
                 .AsNoTracking()
                 .Include(u => u.Roles)
                 .FirstOrDefaultAsync(u => u.UserName == name);

            if(user == null)
                return Result.Fail($"User with name {name} not found.");

            return user.ToUserDto();

        }

        public async Task<Result<UserDto>> GetUserByIdAsync(Guid userId)
        {
            var user = await userContext.Users
                 .AsNoTracking()
                 .Include(u => u.Roles)
                 .FirstOrDefaultAsync(u => u.Id == userId);

            if(user == null)
                return Result.Fail($"User with ID {userId} not found.");

            return user.ToUserDto();

        }




        public async Task<IEnumerable<UserDto>> GetAllUsers()
        {
            var users = await userContext.Users
                .AsNoTracking()
                .Include(u => u.Roles)
                .ToListAsync();

            if (!users.Any())
                return new List<UserDto>();

            var userDtos = users.Select(u => u.ToUserDto());

            return userDtos;
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



        public async Task<string> DeleteUserRole(Guid userId, string roleName)
        {
            if (!Enum.TryParse<UserRoleType>(roleName, ignoreCase: true, out var parsedRole))
            {
                throw new ArgumentException($"Invalid role: {roleName}");
            }

            var existingRole = await userContext.Roles
                .FirstOrDefaultAsync(r => r.UserId == userId && r.Role.ToString() == roleName);

            if(existingRole == null)
                throw new ArgumentException($"Role {roleName} not found for user {userId}.");

            userContext.Roles.Remove(existingRole);

            var newRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Role = parsedRole
            };

            await userContext.SaveChangesAsync();

            //await userContext.Roles.AddAsync(newRole);

            var user = await userContext.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            //user.Roles.Add(newRole);


            var token = tokenProvider.Create(user);

            return token;
        }




        public Task UpdateUserLastLoginAsync(string userId)
        {
            throw new NotImplementedException();
        }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public Guid? CompanyId { get; set; } 
        public string Email { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
    }

    public static class UserMapper
    {
        public static UserDto ToUserDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                CompanyId = user.CompanyId,
                Email = user.Email,
                UserName = user.UserName,
                Roles = user.Roles?.Select(r => r.Role.ToString()).ToList() ?? new()
            };
        }
    }
}
