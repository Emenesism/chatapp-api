using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly ChatDBContext _db;

    public MessageRepository(ChatDBContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Message message)
    {
        await _db.Messages.AddAsync(message);
        await _db.SaveChangesAsync();
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        return await _db.Messages.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Message>> GetPvHistoryAsync(Guid userA, Guid userB, DateTime? before = null, int take = 50)
    {
        var query = _db.Messages
            .Where(m =>
                (m.SenderId == userA && m.ReceiverId == userB) ||
                (m.SenderId == userB && m.ReceiverId == userA));

        if (before.HasValue)
        {
            query = query.Where(m => m.CreatedAt < before.Value);
        }

        return await query
            .OrderByDescending(m => m.CreatedAt)
            .Take(take)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

}
