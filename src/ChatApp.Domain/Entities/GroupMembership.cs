namespace ChatApp.Domain.Entities;

public class GroupMembership
{
    public Guid GroupId { get; private set; }
    public GroupChat Group { get; private set; } = null!;

    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public DateTime JoinedAt { get; private set; }

    public GroupMembership(Guid groupId, Guid userId)
    {
        if (groupId == Guid.Empty) throw new ArgumentException("Group id is required", nameof(groupId));
        if (userId == Guid.Empty) throw new ArgumentException("User id is required", nameof(userId));

        GroupId = groupId;
        UserId = userId;
        JoinedAt = DateTime.UtcNow;
    }
}
