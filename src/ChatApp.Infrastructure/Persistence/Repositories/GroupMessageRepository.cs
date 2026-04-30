using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

public class GroupMessageRepository : IGroupMessageRepository
{
    private readonly ChatDBContext _db;

    public GroupMessageRepository(ChatDBContext db)
    {
        _db = db;
    }

    public async Task AddAsync(GroupMessage message)
    {
        await _db.GroupMessages.AddAsync(message);
        await _db.SaveChangesAsync();
    }

    public async Task<List<GroupMessage>> GetHistoryAsync(Guid groupId, DateTime? before = null, int take = 50)
    {
        var query = _db.GroupMessages.Where(gm => gm.GroupId == groupId);

        if (before.HasValue)
        {
            query = query.Where(gm => gm.CreatedAt < before.Value);
        }

        return await query
            .OrderByDescending(gm => gm.CreatedAt)
            .Take(take)
            .OrderBy(gm => gm.CreatedAt)
            .ToListAsync();
    }
}
