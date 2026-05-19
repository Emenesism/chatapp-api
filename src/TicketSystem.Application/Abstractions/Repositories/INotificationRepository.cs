

using TicketSystem.Domain.Entities;

namespace TicketSystem.Application.Abstractions.Repositories;



public interface INotificationRepository
{
    Task AddAsync(Notification notification);
    Task<List<Notification>> GetUserNotification(string userId);
    Task MarkUserNotificationsAsReadAsync(string userId);
}
