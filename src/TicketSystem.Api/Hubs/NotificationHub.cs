using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TicketSystem.Api.Hubs;

[Authorize]
public class NotificationHub : Hub
{
}
