

namespace TicketSystem.Application.Dtos.Notification;


public sealed class GetUserNotificationResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
