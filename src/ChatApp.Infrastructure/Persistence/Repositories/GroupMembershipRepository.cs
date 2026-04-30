using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

public class GroupMembershipRepository : IGroupMembershipRepository
{
    private readonly ChatDBContext _db;

    public GroupMembershipRepository(ChatDBContext db)
    {
        _db = db;
    }

    public async Task<bool> IsMemberAsync(Guid groupId, Guid userId)
    {
        return await _db.GroupMemberships.AnyAsync(gm => gm.GroupId == groupId && gm.UserId == userId);
    }

    public async Task AddAsync(Guid groupId, Guid userId)
    {
        var membership = new GroupMembership(groupId, userId);
        await _db.GroupMemberships.AddAsync(membership);
        await _db.SaveChangesAsync();
    }

    public async Task<List<GroupChat>> GetUserGroupsAsync(Guid userId)
    {
        return await _db.GroupMemberships
            .Where(gm => gm.UserId == userId)
            .Select(gm => gm.Group)
            .ToListAsync();
    }
}
