using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Repositories;

public interface IMessageRepository
{
    Task AddAsync(Message message);
    Task<Message?> GetByIdAsync(Guid id);
}
