namespace Foruscorp.Push.Features.Notifications
{
    public record NotificationDto(
        Guid Id,
        string Title,
        string Body,
        IDictionary<string, object> Payload,
        string Status,
        IReadOnlyCollection<NotificationRecipientDto> Recipients
    );
}
