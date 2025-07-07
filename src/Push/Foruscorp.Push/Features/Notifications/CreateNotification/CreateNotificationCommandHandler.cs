using Foruscorp.Push.Domain.PushNotifications;
using Foruscorp.Push.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Features.Notifications.CreateNotification
{
    public record CreateNotificationCommand(
    string Title,
    string Body,
    IDictionary<string, object> Payload,
    IEnumerable<Guid> DeviceIds) : IRequest<Guid>;

    public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand, Guid>
    {
        private readonly PushNotificationsContext _db;
        public CreateNotificationCommandHandler(PushNotificationsContext db) => _db = db;

        public async Task<Guid> Handle(CreateNotificationCommand cmd, CancellationToken ct)
        {
            var content = new NotificationContent(cmd.Title, cmd.Body);
            var payload = new NotificationPayload(new Dictionary<string, object>(cmd.Payload));
            var notification = new Notification(content, payload);

            var devices = await _db.Devices
                .Where(d => cmd.DeviceIds.Contains(d.Id))
                .ToListAsync(ct);
            foreach (var d in devices)
                notification.AddRecipient(d);

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync(ct);
            return notification.Id;
        }
    }
}
