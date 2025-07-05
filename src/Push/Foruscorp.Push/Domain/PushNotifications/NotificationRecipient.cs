using Foruscorp.Push.Domain.Devices;

namespace Foruscorp.Push.Domain.PushNotifications
{
    public class NotificationRecipient
    {
        public Guid Id { get; private set; }
        public Guid NotificationId { get; private set; }
        public Notification Notification { get; private set; }
        public Guid DeviceId { get; private set; }
        public Device Device { get; private set; }
        public RecipientStatus Status { get; private set; }
        public DateTime? DeliveredAt { get; private set; }
        public string FailureReason { get; private set; }

        private NotificationRecipient() { } // Для ORM

        public NotificationRecipient(Notification notification, Device device)
        {
            Id = Guid.NewGuid();
            Notification = notification;
            NotificationId = notification.Id;
            Device = device;
            DeviceId = device.Id;
            Status = RecipientStatus.Pending;
        }

        public void MarkDelivered(DateTime deliveredAt)
        {
            Status = RecipientStatus.Delivered;
            DeliveredAt = deliveredAt;
        }

        public void MarkFailed(string reason)
        {
            Status = RecipientStatus.Failed;
            FailureReason = reason;
        }
    }
}
