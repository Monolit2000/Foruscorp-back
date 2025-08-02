namespace Foruscorp.Auth.Contruct
{
    public interface IUserService
    {
        Task SetUserRole(Guid userId, string roleName);
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
