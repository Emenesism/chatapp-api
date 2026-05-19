using TicketSystem.Application.Dtos;
using TicketSystem.Application.Dtos.Notification;

namespace TicketSystem.Application.Common.Interface;

public interface INotificationSender
{
    Task SendToUserAsync(Guid userId, NotificationDto notification);
}
