using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ChatDBContext _db;

    public UserRepository(ChatDBContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByPhoneAsync(string phone)
    {
        return await _db.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phone);
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _db.Users.ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }
}
