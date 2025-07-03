using Foruscorp.FuelRoutes.Auth.Domain.Users;

namespace Foruscorp.FuelRoutes.Auth.Contruct
{
    public interface ITokenProvider
    {
        public string Create(User user);
    }
}
