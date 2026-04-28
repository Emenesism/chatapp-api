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

}
