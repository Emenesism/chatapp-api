namespace ChatApp.Domain.Entities;

public class Message
{
    public const int MaxContentLength = 2000;

    public Guid Id { get; private set; }

    // PV fields
    public Guid SenderId { get; private set; }
    public User Sender { get; private set; } = null!;

    public Guid ReceiverId { get; private set; }
    public User Receiver { get; private set; } = null!;

    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public Message(Guid senderId, Guid receiverId, string content)
    {
        if (senderId == Guid.Empty) throw new ArgumentException("Sender id is required", nameof(senderId));
        if (receiverId == Guid.Empty) throw new ArgumentException("Receiver id is required", nameof(receiverId));
        if (senderId == receiverId) throw new ArgumentException("Sender and receiver cannot be the same");
        if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content is required", nameof(content));

        var trimmedContent = content.Trim();
        if (trimmedContent.Length > MaxContentLength)
            throw new ArgumentException($"Content cannot be longer than {MaxContentLength} characters", nameof(content));

        Id = Guid.NewGuid();
        SenderId = senderId;
        ReceiverId = receiverId;
        Content = trimmedContent;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
