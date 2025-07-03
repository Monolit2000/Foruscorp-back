using Foruscorp.Auth.Domain.Users;

namespace Foruscorp.Auth.Contruct
{
    public interface ITokenProvider
    {
        public string Create(User user);
    }
}
