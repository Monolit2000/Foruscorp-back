using MediatR;
using Foruscorp.Push.Contruct;
using Microsoft.EntityFrameworkCore;
using Foruscorp.Push.Infrastructure.Database;
using Foruscorp.Push.Domain.PushNotifications;

namespace Foruscorp.Push.Features.Notifications.CreateAndSendNearStationNotification
{

    public record CreateAndSendNearStationNotificationCommand(Guid UserId, Guid StationId, double DistanceKm) : IRequest;

    public class CreateAndSendNearStationNotificationCommandHandler(
        IExpoPushService pushService,
        PushNotificationsContext context): IRequestHandler<CreateAndSendNearStationNotificationCommand>
    {
        const double KmToMiles = 0.621371;
        public async Task Handle(CreateAndSendNearStationNotificationCommand request, CancellationToken cancellationToken)
        {

            var content = new NotificationContent("Fuel station", $"Refuel in {Math.Round(request.DistanceKm * KmToMiles, 2)} miles");
            var payloadData = new Dictionary<string, object>
            {
                ["StationId"] = request.StationId,
                ["NotificationType"] = "NearFuelStation",
                ["DistanceMiles"] = Math.Round(request.DistanceKm * KmToMiles, 2),
                ["CreatedAtUtc"] = DateTime.UtcNow,
            };

            var payload = new NotificationPayload(payloadData);

            var notification = new Notification(content, payload);

            var devices = await context.Devices
                .Where(d => d.UserId == request.UserId)
                .ToListAsync(cancellationToken);
            foreach (var d in devices)
                notification.AddRecipient(d);


            context.Notifications.Add(notification);

            await SendToPendingRecipientsAsync(notification, cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task SendToPendingRecipientsAsync(Notification notification, CancellationToken ct)
        {
            foreach (var rec in notification.Recipients.ToList())
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
            }
        }

    }
}
