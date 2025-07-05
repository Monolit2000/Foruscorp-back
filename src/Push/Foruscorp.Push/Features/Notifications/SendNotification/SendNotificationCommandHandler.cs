using Foruscorp.Push.Contruct;
using Foruscorp.Push.Domain.PushNotifications;
using Foruscorp.Push.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Features.Notifications.SendNotification
{
    public record SendNotificationCommand(Guid NotificationId) : IRequest;
    public class SendNotificationCommandHandler(
        IExpoPushService expo,
        PushNotificationsContext context) : IRequestHandler<SendNotificationCommand>
    {

        public async Task Handle(SendNotificationCommand cmd, CancellationToken ct)
        {
            var notification = await context.Notifications
                .Include(n => n.Recipients)
                .ThenInclude(r => r.Device)
                .FirstOrDefaultAsync(n => n.Id == cmd.NotificationId, ct)
                ?? throw new KeyNotFoundException("Notification not found");


            await SendToPendingRecipientsAsync(notification, ct);


            if (notification.Recipients.All(r => r.Status == RecipientStatus.Delivered))
                notification.MarkAsSent();
            else if (notification.Recipients.Any(r => r.Status == RecipientStatus.Failed))
                notification.MarkAsFailed();

            await context.SaveChangesAsync(ct);
        }


        private async Task SendToPendingRecipientsAsync(Notification notification, CancellationToken ct)
        {
            foreach (var rec in notification.Recipients.ToList())
            {
                try
                {
                    await expo.SendAsync(
                        rec.Device.Token.Value,
                        notification.Content.Title,
                        notification.Content.Body,
                        notification.Payload.Data);

                    rec.MarkDelivered(DateTime.UtcNow);
                }
                catch (Exception ex)
                {
                    rec.MarkFailed(ex.Message);
                }
            }
        }
    }
}
