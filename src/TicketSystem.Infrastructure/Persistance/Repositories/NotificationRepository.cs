using Microsoft.EntityFrameworkCore;
using TicketSystem.Application.Abstractions.Repositories;
using TicketSystem.Domain.Entities;
using TicketSystem.Infrastructure.Persistance.Configuration;

namespace TicketSystem.Infrastructure.Persistance.Repositories;



public class NotificationRepository(AppDbContext db) : INotificationRepository
{
    private readonly AppDbContext _db = db;

    public async Task AddAsync(Notification notification)
    {
        await _db.Notifications.AddAsync(notification);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetUserNotification(string userId)
    {
        return await _db.Notifications
        .AsNoTracking()
        .Where(s =>
            s.UserId == userId &&
            s.IsRead == false)
        .OrderByDescending(s => s.CreatedAt)
        .ToListAsync();
    }

    public async Task MarkUserNotificationsAsReadAsync(string userId)
    {
        await _db.Notifications
            .Where(s => s.UserId == userId && s.IsRead == false)
            .ExecuteUpdateAsync(setters => setters.SetProperty(s => s.IsRead, true));
    }
}
