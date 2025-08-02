namespace Foruscorp.Auth.Domain.Users
{
    public class UserRole
    {
        public Guid Id { get; set; }
        public UserRoleType Role { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
