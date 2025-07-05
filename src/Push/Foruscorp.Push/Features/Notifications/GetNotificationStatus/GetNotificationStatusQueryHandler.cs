using Foruscorp.Push.Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Features.Notifications.GetNotificationStatus
{
    public record GetNotificationStatusQuery(Guid NotificationId) : IRequest<NotificationDto>;

    public class GetNotificationStatusQueryHandler(PushNotificationsContext context) : IRequestHandler<GetNotificationStatusQuery, NotificationDto>
    {
        public async Task<NotificationDto> Handle(GetNotificationStatusQuery request, CancellationToken cancellationToken)
        {
            var n = await context.Notifications
                .Include(x => x.Recipients)
                .FirstOrDefaultAsync(x => x.Id == request.NotificationId, cancellationToken)
                ?? throw new KeyNotFoundException("Notification not found");

            var dto = new NotificationDto(
                n.Id,
                n.Content.Title,
                n.Content.Body,
                n.Payload.Data,
                n.Status.ToString(),
                n.Recipients
                 .Select(r => new NotificationRecipientDto(
                     r.DeviceId,
                     r.Status.ToString(),
                     r.DeliveredAt,
                     r.FailureReason
                 ))
                 .ToList()
            );
            return dto;
        }
    }
}
