namespace ChatApp.Domain.Entities;

public class GroupMessage
{
    public const int MaxContentLength = 2000;

    public Guid Id { get; private set; }
    public Guid GroupId { get; private set; }
    public GroupChat Group { get; private set; } = null!;

    public Guid SenderId { get; private set; }
    public User Sender { get; private set; } = null!;

    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public GroupMessage(Guid groupId, Guid senderId, string content)
    {
        if (groupId == Guid.Empty) throw new ArgumentException("Group id is required", nameof(groupId));
        if (senderId == Guid.Empty) throw new ArgumentException("Sender id is required", nameof(senderId));
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content is required", nameof(content));

        var trimmedContent = content.Trim();
        if (trimmedContent.Length > MaxContentLength)
            throw new ArgumentException($"Content cannot be longer than {MaxContentLength} characters", nameof(content));

        Id = Guid.NewGuid();
        GroupId = groupId;
        SenderId = senderId;
        Content = trimmedContent;
        CreatedAt = DateTime.UtcNow;
    }
}
