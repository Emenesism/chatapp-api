using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

public class GroupChatRepository : IGroupChatRepository
{
    private readonly ChatDBContext _db;

    public GroupChatRepository(ChatDBContext db)
    {
        _db = db;
    }

    public async Task<GroupChat?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return null;
        var normalizedUsername = username.Trim().ToLowerInvariant();
        return await _db.GroupChats.FirstOrDefaultAsync(g => g.Username == normalizedUsername);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username)) return false;
        var normalizedUsername = username.Trim().ToLowerInvariant();
        return await _db.GroupChats.AnyAsync(g => g.Username == normalizedUsername);
    }

    public async Task AddAsync(GroupChat group)
    {
        await _db.GroupChats.AddAsync(group);
        await _db.SaveChangesAsync();
    }
}
