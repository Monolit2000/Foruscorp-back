namespace Foruscorp.Auth.Domain.Users
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Token { get; set; }
        public Guid UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public DateTime Created { get; set; }
        public bool IsRevoked { get; set; }
        public bool IsUsed { get; set; }

        public bool IsActive => !IsRevoked && !IsUsed && ExpiresAt > DateTime.UtcNow;
    }
}
