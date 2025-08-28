using Foruscorp.Push.Contruct;
using Foruscorp.Push.Domain.PushNotifications;
using Foruscorp.Push.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Features.Notifications.CreateAndSendRouteOfferNotification
{

    public record CreateAndSendRouteOfferNotificationCommand(Guid UserId, Guid RouteId) : IRequest;

    public class CreateAndSendRouteOfferNotificationCommandHandler(
        PushNotificationsContext context,
        IExpoPushService pushService) : IRequestHandler<CreateAndSendRouteOfferNotificationCommand>
    {
        public async Task Handle(CreateAndSendRouteOfferNotificationCommand request, CancellationToken cancellationToken)
        {
            var content = new NotificationContent("Manager route", $"You have a new route suggestion.");
            var payloadData = new Dictionary<string, object>
            {
                ["RouteId"] = request.RouteId,
                ["NotificationType"] = "RouteOffer",
                ["CreatedAtUtc"] = DateTime.UtcNow,
            };

            var payload = new NotificationPayload(payloadData);

            var notification = new Notification(content, payload);

            var devices = await context.Devices
                .Where(d => d.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            var activeDevices = devices.Where(d => d.IsActive).ToList();

            if (!activeDevices.Any())
                return;

            foreach (var d in devices)
                notification.AddRecipient(d);


            context.Notifications.Add(notification);

            await SendToPendingRecipientsAsync(notification, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task SendToPendingRecipientsAsync(Notification notification, CancellationToken ct)
        {
            var tasks = notification.Recipients
                .Select(async rec =>
                {
                    try
                    {
                        await pushService.SendAsync(
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
                })
                .ToList();

            await Task.WhenAll(tasks);
        }

    }
}
