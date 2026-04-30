using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Repositories;

public interface IGroupChatRepository
{
    Task<GroupChat?> GetByUsernameAsync(string username);
    Task<bool> ExistsByUsernameAsync(string username);
    Task AddAsync(GroupChat group);
}
