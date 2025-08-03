using Foruscorp.Auth.Controllers;
using Foruscorp.Auth.Domain.Users;
using Foruscorp.Auth.Servises;
using UserAuthDto = Foruscorp.Auth.Controllers.UserAuthDto;

namespace Foruscorp.Auth.Contruct
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(UserAuthDto request);
        Task<LoginResponce> LoginAsync(UserLoginDto request);
    }
}
