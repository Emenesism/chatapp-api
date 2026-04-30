using ChatApp.Application.Abstractions.Repositories;

namespace ChatApp.Application.Messages.Groups;

public class JoinGroupByUsernameHandler
{
    private readonly IGroupChatRepository _groups;
    private readonly IGroupMembershipRepository _memberships;

    public JoinGroupByUsernameHandler(IGroupChatRepository groups, IGroupMembershipRepository memberships)
    {
        _groups = groups;
        _memberships = memberships;
    }

    public async Task<JoinGroupByUsernameResult> Handle(Guid currentUserId, JoinGroupByUsernameRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (currentUserId == Guid.Empty) throw new ArgumentException("Current user id is required", nameof(currentUserId));
        if (string.IsNullOrWhiteSpace(request.Username)) throw new ArgumentException("Group username is required", nameof(request.Username));

        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var group = await _groups.GetByUsernameAsync(normalizedUsername);
        if (group is null) throw new InvalidOperationException("Group not found");

        var alreadyMember = await _memberships.IsMemberAsync(group.Id, currentUserId);
        if (!alreadyMember)
            await _memberships.AddAsync(group.Id, currentUserId);

        return new JoinGroupByUsernameResult(group.Id, group.Name, group.Username, alreadyMember);
    }
}
