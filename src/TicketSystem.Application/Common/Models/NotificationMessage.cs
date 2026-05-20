namespace TicketSystem.Application.Common.Models;

public sealed class NotificationMessage
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
