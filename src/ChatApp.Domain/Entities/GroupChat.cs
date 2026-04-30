namespace ChatApp.Domain.Entities;

public class GroupChat
{
    public const int MaxNameLength = 100;
    public const int MaxUsernameLength = 50;

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Username { get; private set; }
    public Guid CreatorId { get; private set; }
    public User Creator { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    public ICollection<GroupMembership> Members { get; private set; } = new List<GroupMembership>();
    public ICollection<GroupMessage> Messages { get; private set; } = new List<GroupMessage>();

    public GroupChat(Guid creatorId, string name, string username)
    {
        if (creatorId == Guid.Empty) throw new ArgumentException("Creator id is required", nameof(creatorId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Group name is required", nameof(name));
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Group username is required", nameof(username));

        var normalizedName = name.Trim();
        var normalizedUsername = username.Trim().ToLowerInvariant();

        if (normalizedName.Length > MaxNameLength)
            throw new ArgumentException($"Group name cannot be longer than {MaxNameLength} characters", nameof(name));
        if (normalizedUsername.Length > MaxUsernameLength)
            throw new ArgumentException($"Group username cannot be longer than {MaxUsernameLength} characters", nameof(username));

        Id = Guid.NewGuid();
        CreatorId = creatorId;
        Name = normalizedName;
        Username = normalizedUsername;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }
}
