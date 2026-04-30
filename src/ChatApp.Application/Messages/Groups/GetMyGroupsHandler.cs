using ChatApp.Application.Abstractions.Repositories;

namespace ChatApp.Application.Messages.Groups;

public class GetMyGroupsHandler
{
    private readonly IGroupMembershipRepository _memberships;

    public GetMyGroupsHandler(IGroupMembershipRepository memberships)
    {
        _memberships = memberships;
    }

    public async Task<List<MyGroupDto>> Handle(Guid currentUserId)
    {
        if (currentUserId == Guid.Empty) throw new ArgumentException("Current user id is required", nameof(currentUserId));

        var groups = await _memberships.GetUserGroupsAsync(currentUserId);
        return groups.Select(g => new MyGroupDto(g.Id, g.Name, g.Username)).ToList();
    }
}
