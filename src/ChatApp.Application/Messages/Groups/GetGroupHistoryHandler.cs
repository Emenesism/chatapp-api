using ChatApp.Application.Abstractions.Repositories;

namespace ChatApp.Application.Messages.Groups;

public class GetGroupHistoryHandler
{
    private readonly IGroupChatRepository _groups;
    private readonly IGroupMembershipRepository _memberships;
    private readonly IGroupMessageRepository _messages;

    public GetGroupHistoryHandler(
        IGroupChatRepository groups,
        IGroupMembershipRepository memberships,
        IGroupMessageRepository messages)
    {
        _groups = groups;
        _memberships = memberships;
        _messages = messages;
    }

    public async Task<IReadOnlyList<GroupMessageDto>> Handle(Guid currentUserId, GetGroupHistoryQuery query)
    {
        if (query is null) throw new ArgumentNullException(nameof(query));
        if (currentUserId == Guid.Empty) throw new ArgumentException("Current user id is required", nameof(currentUserId));
        if (string.IsNullOrWhiteSpace(query.GroupUsername)) throw new ArgumentException("Group username is required", nameof(query.GroupUsername));

        var normalizedUsername = query.GroupUsername.Trim().ToLowerInvariant();
        var group = await _groups.GetByUsernameAsync(normalizedUsername);
        if (group is null) throw new InvalidOperationException("Group not found");

        var isMember = await _memberships.IsMemberAsync(group.Id, currentUserId);
        if (!isMember) throw new UnauthorizedAccessException("You are not a member of this group");

        var take = Math.Clamp(query.Take, 1, 100);
        var messages = await _messages.GetHistoryAsync(group.Id, query.Before, take);

        return messages
            .Select(m => new GroupMessageDto(m.Id, m.GroupId, group.Username, m.SenderId, m.Content, m.CreatedAt))
            .ToList();
    }
}
