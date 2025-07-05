using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Foruscorp.Push.Domain.PushNotifications
{
    public record NotificationPayload(Dictionary<string, object> Data);
}
