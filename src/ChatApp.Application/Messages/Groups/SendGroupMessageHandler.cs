using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Messages.Groups;

public class SendGroupMessageHandler
{
    private readonly IGroupChatRepository _groups;
    private readonly IGroupMembershipRepository _memberships;
    private readonly IGroupMessageRepository _messages;

    public SendGroupMessageHandler(
        IGroupChatRepository groups,
        IGroupMembershipRepository memberships,
        IGroupMessageRepository messages)
    {
        _groups = groups;
        _memberships = memberships;
        _messages = messages;
    }

    public async Task<SendGroupMessageResult> Handle(Guid currentUserId, SendGroupMessageRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (currentUserId == Guid.Empty) throw new ArgumentException("Current user id is required", nameof(currentUserId));
        if (string.IsNullOrWhiteSpace(request.GroupUsername)) throw new ArgumentException("Group username is required", nameof(request.GroupUsername));

        var normalizedUsername = request.GroupUsername.Trim().ToLowerInvariant();
        var group = await _groups.GetByUsernameAsync(normalizedUsername);
        if (group is null) throw new InvalidOperationException("Group not found");

        var isMember = await _memberships.IsMemberAsync(group.Id, currentUserId);
        if (!isMember) throw new UnauthorizedAccessException("You are not a member of this group");

        var message = new GroupMessage(group.Id, currentUserId, request.Content);
        await _messages.AddAsync(message);

        return new SendGroupMessageResult(
            message.Id,
            message.GroupId,
            group.Username,
            message.SenderId,
            message.Content,
            message.CreatedAt);
    }
}
