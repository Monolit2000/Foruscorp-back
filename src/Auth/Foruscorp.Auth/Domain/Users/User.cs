
using System.ComponentModel.DataAnnotations.Schema;

namespace Foruscorp.Auth.Domain.Users
{
    public class User
    {
        public Guid? CompanyId { get; private set; }
        public Guid Id { get; private set; }
        public string Email { get; private set; }
        public string UserName { get; private set; }
        public string PasswordHash { get; set; }

        public List<UserRole> Roles { get; set; } = [];

        private User(string email, string userName) 
        {
            Id = Guid.NewGuid();
            Email = email;
            UserName = userName;
        }

        public static User CreateNew(string email, string userName)
            => new User(email, userName);

        public void SetCompanyId(Guid companyId)
        {
            CompanyId = companyId;
        }

        public void SetDriverRole()
        {
            Roles.Add(new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = Id,
                Role = UserRoleType.Driver
            });
        }
    }
}
