using ChatApp.Domain.Entities;

namespace ChatApp.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByPhoneAsync(string phone);
    Task AddAsync(User user);
}
