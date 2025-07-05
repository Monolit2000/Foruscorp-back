using Foruscorp.Push.Infrastructure.Database;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace Foruscorp.Push.Features.Notifications.GetNotificationByUserId
{
    public record GetNotificationByUserIdQuery(Guid? userId) : IRequest<IEnumerable<NotificationDto>>;


    public class GetNotificationByUserIdQueryHandler(
        PushNotificationsContext pushNotificationsContext) : IRequestHandler<GetNotificationByUserIdQuery, IEnumerable<NotificationDto>>
    {
        public async Task<IEnumerable<NotificationDto>> Handle(GetNotificationByUserIdQuery request, CancellationToken cancellationToken)
        {
            var notifications = await pushNotificationsContext.Notifications
                .Include(n => n.Recipients)
                .ThenInclude(r => r.Device)
                .Where(n => n.Recipients.Any(r => r.Device.UserId == request.userId))
                .AsSplitQuery()
                .AsNoTracking()
                .Select(n => new NotificationDto(
                    n.Id,
                    n.Content.Title,
                    n.Content.Body,
                    n.Payload.Data,
                    n.Status.ToString(),
                    null
                    //n.Recipients.Select(r => new NotificationRecipientDto(
                    //    r.DeviceId,
                    //    r.Status.ToString(),
                    //    r.DeliveredAt,
                    //    r.FailureReason
                    //)).ToList()
                )).ToListAsync(cancellationToken);

            if(!notifications.Any())
                return new List<NotificationDto>(); 

            return notifications;

        }
    }
}
