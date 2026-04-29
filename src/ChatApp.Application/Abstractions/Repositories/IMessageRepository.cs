using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<Message?> GetByIdAsync(Guid id);
    Task<List<Message>> GetPvHistoryAsync(Guid userA, Guid userB, DateTime? before = null, int take = 50);
}
