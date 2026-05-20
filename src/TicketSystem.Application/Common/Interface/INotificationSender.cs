using TicketSystem.Application.Common.Models;

namespace TicketSystem.Application.Common.Interface;

public interface INotificationSender
{
    Task SendToUserAsync(Guid userId, NotificationMessage notification);
}
