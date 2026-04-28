namespace ChatApp.Domain.Entities;

public class Message
{
    public Guid Id { get; private set; }
    public Guid ChatId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    //Contructor
    // IDE suggest that we can use "Use primary constructor" but for now we pass becuase maybe it cuase incompatibilty with EF.

    public Message(Guid chatId, Guid senderId, string content)
    {
        Id = Guid.NewGuid();
        SenderId = senderId;
        ChatId = chatId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

}
