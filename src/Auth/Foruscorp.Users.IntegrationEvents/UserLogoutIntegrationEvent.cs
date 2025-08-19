using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.Users.IntegrationEvents
{
    public class UserLogoutIntegrationEvent
    {
        public Guid UserId { get; set; }
        public string ExpoToken { get; set; } = string.Empty;
        public DateTime LogoutAt { get; set; }
    }
}
