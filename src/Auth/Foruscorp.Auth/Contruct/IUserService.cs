using FluentResults;
using Foruscorp.Auth.Servises;

namespace Foruscorp.Auth.Contruct
{
    public interface IUserService
    {
        Task<string> SetUserRole(Guid userId, string roleName);
        Task<string> DeleteUserRole(Guid userId, string roleName);
        Task<string> SetCompanyId(Guid userId, Guid companyId);
        Task<IEnumerable<UserDto>> GetAllUsers();

        Task<Result<UserDto>> GetUserByNameAsync(string name);
        Task<Result<UserDto>> GetUserByIdAsync(Guid userId);

        Task<bool> IsUserAuthenticatedAsync(string userId, string password);
        Task<string> GetUserNameAsync(string userId);
        Task<string> GetUserEmailAsync(string userId);
        Task<string> GetUserPhoneAsync(string userId);
        Task<bool> IsUserAdminAsync(string userId);
        Task<bool> IsUserActiveAsync(string userId);
        Task UpdateUserLastLoginAsync(string userId);
        Task CreateUserAsync(string userName, string email, string phone, string password);
        Task UpdateUserAsync(string userId, string userName, string email, string phone);
        Task DeleteUserAsync(string userId);
    }
}
