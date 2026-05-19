namespace TicketSystem.Domain.Entities;

public class Notification(string userId, string title, string body)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = userId;
    public string Title { get; set; } = title;
    public string Body { get; set; } = body;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
