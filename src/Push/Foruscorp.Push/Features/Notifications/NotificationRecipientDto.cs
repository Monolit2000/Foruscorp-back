namespace Foruscorp.Push.Features.Notifications
{
    public record NotificationRecipientDto(Guid DeviceId, string Status, DateTime? DeliveredAt, string FailureReason);

}
