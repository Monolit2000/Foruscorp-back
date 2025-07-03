using Foruscorp.Auth.Controllers;
using Foruscorp.Auth.Domain.Users;

namespace Foruscorp.Auth.Contruct
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(UserDto request);
        Task<string> LoginAsync(UserLoginDto request);
    }
}
