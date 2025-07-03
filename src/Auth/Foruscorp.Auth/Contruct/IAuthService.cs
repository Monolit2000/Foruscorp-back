using Foruscorp.Auth.Controllers;
using Foruscorp.Auth.Domain.Users;
using Foruscorp.Auth.Servises;

namespace Foruscorp.Auth.Contruct
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(UserDto request);
        Task<LoginResponce> LoginAsync(UserLoginDto request);
    }
}
