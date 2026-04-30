using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Repositories;

public interface IGroupMessageRepository
{
    Task AddAsync(GroupMessage message);
    Task<List<GroupMessage>> GetHistoryAsync(Guid groupId, DateTime? before = null, int take = 50);
}
