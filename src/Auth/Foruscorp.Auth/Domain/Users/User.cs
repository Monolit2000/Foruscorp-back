
namespace Foruscorp.Auth.Domain.Users
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; }
        public string UserName { get; private set; }
        public string PasswordHash { get; set; }

        private User() { }

        public static User CreateNew(string email, string userName)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                UserName = userName,
            };
        }
    }
}
