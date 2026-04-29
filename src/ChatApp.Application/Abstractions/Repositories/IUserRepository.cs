using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByPhoneAsync(string phone);
    Task<User?> GetByIdAsync(Guid id);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
}
