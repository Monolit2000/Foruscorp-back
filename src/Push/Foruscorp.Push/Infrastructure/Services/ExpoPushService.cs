using ExpoCommunityNotificationServer.Client;
using ExpoCommunityNotificationServer.Models;

// from Community.Expo.Server.SDK
using Foruscorp.Push.Contruct;

namespace Foruscorp.Push.Infrastructure.Services
{
    public class ExpoPushService : IExpoPushService
    {
        private readonly IPushApiClient _expoClient;

        public ExpoPushService(IPushApiClient expoClient)
        {
            _expoClient = expoClient;
        }

        public async Task SendAsync(
            string pushToken,
            string title,
            string body,
            IDictionary<string, object> data)
        {
            var pushReq = new PushTicketRequest
            {
                PushTo = new List<string> { pushToken },
                PushTitle = title,
                PushBody = body,
                PushData = data,
                // По желанию: PushChannelId, PushBadgeCount и т.д.
            };

            var ticketResponse = await _expoClient.SendPushAsync(pushReq);

            // Отбираем не-OK
            var failed = ticketResponse.PushTicketStatuses
                .Where(s => !string.Equals(s.TicketStatus, "ok", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (failed.Any())
            {
                // Собираем детали ошибок
                var details = string.Join("; ", failed.Select(s =>
                {
                    // обычно у s есть свойства .Status, .Message и .Details.Error
                    var errCode = s.Details?.Error ?? "UnknownError";
                    var errMsg = s.TicketMessage ?? "[no message]";
                    return $"{errCode}: {errMsg}";
                }));

                throw new InvalidOperationException(
                    $"Error sending Expo notification(s): {details}"
                );
            }
        }
    }
}
