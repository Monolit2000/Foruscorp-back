using Foruscorp.Push.Domain.PushNotifications;
using MassTransit.SqlTransport;

namespace Foruscorp.Push.Domain.Devices
{
    public class Device
    {
        public Guid Id { get; private set; }                   
        public Guid? UserId { get; private set; }                
        public ExpoPushToken Token { get; private set; }         
        public DateTime RegisteredAt { get; private set; }        
        public bool IsActive { get; private set; } = true;  
        private Device() { } 

        public Device(ExpoPushToken token, Guid? userId = null)
        {
            Id = Guid.NewGuid();    
            Token = token ?? throw new ArgumentNullException(nameof(token));
            UserId = userId;
            RegisteredAt = DateTime.UtcNow;
        }

        public void UpdateToken(ExpoPushToken newToken)
        {
            if (newToken == null) throw new ArgumentNullException(nameof(newToken));
            Token = newToken;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;

    }
}
