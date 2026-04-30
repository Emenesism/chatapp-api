using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Messages.Groups;

public class CreateGroupChatHandler
{
    private readonly IGroupChatRepository _groups;
    private readonly IGroupMembershipRepository _memberships;

    public CreateGroupChatHandler(IGroupChatRepository groups, IGroupMembershipRepository memberships)
    {
        _groups = groups;
        _memberships = memberships;
    }

    public async Task<CreateGroupChatResult> Handle(Guid currentUserId, CreateGroupChatRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (currentUserId == Guid.Empty) throw new ArgumentException("Current user id is required", nameof(currentUserId));
        if (string.IsNullOrWhiteSpace(request.Username)) throw new ArgumentException("Group username is required", nameof(request.Username));

        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        if (await _groups.ExistsByUsernameAsync(normalizedUsername))
            throw new InvalidOperationException("Group username already exists");

        var group = new GroupChat(currentUserId, request.Name, normalizedUsername);
        await _groups.AddAsync(group);
        await _memberships.AddAsync(group.Id, currentUserId);

        return new CreateGroupChatResult(group.Id, group.Name, group.Username, group.CreatorId, group.CreatedAt);
    }
}
