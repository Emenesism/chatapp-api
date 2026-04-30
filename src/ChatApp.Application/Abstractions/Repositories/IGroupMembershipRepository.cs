using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Repositories;

public interface IGroupMembershipRepository
{
    Task<bool> IsMemberAsync(Guid groupId, Guid userId);
    Task AddAsync(Guid groupId, Guid userId);
    Task<List<GroupChat>> GetUserGroupsAsync(Guid userId);
}
