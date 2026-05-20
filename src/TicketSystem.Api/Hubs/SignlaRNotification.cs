using Microsoft.AspNetCore.SignalR;
using TicketSystem.Application.Common.Interface;
using TicketSystem.Application.Common.Models;

namespace TicketSystem.Api.Hubs;

public class SignalRNotificationSender(IHubContext<NotificationHub> hubContext) : INotificationSender
{
    private readonly IHubContext<NotificationHub> _hubContext = hubContext;

    public async Task SendToUserAsync(Guid userId, NotificationMessage notification)
    {
        await _hubContext.Clients.User(userId.ToString())
            .SendAsync("ReceiveNotification", notification);
    }
}
