namespace Foruscorp.Users.IntegrationEvents
{
    public class NewUserRegistratedIntegrationEvent
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
    }
}
