using RabbitMQ.Client;

namespace Foruscorp.Push.Domain.Users
{
    public class User
    {
        public Guid UserId { get; set; }
        public Guid TokenId { get; set; }

        public User()
        {
                
        }
    }
}
