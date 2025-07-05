using Foruscorp.Push.Domain.Devices;
using MassTransit.Serialization.JsonConverters;

namespace Foruscorp.Push.Domain.PushNotifications
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public NotificationContent Content { get; private set; }            // ValueObject
        public NotificationPayload Payload { get; private set; }            // ValueObject
        public DateTime CreatedAt { get; private set; }
        public NotificationStatus Status { get; private set; }

        private readonly List<NotificationRecipient> _recipients = new();
        public IReadOnlyCollection<NotificationRecipient> Recipients => _recipients.AsReadOnly();

        private Notification() { } 

        public Notification(NotificationContent content, NotificationPayload payload)
        {
            Id = Guid.NewGuid();
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
            CreatedAt = DateTime.UtcNow;
            Status = NotificationStatus.Pending;
        }

        public void AddRecipient(Device device)
        {
            if (device == null) throw new ArgumentNullException(nameof(device));
            _recipients.Add(new NotificationRecipient(this, device));
        }

        public void MarkAsSent()
            => Status = NotificationStatus.Sent;

        public void MarkAsFailed()
            => Status = NotificationStatus.Failed;
    }
}
