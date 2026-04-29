using ChatApp.Application.Messages.PV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.WebApi.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly SendPvMessageHandler _sendPvMessage;

    public ChatHub(SendPvMessageHandler sendPvMessage)
    {
        _sendPvMessage = sendPvMessage;
    }

    public static string UserGroup(Guid userId) => $"user:{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId(Context);
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        await base.OnConnectedAsync();
    }

    public async Task SendPv(Guid receiverId, string content)
    {
        var senderId = GetUserId(Context);
        var result = await _sendPvMessage.Handle(senderId, new SendPvMessageRequest(receiverId, content));
        var payload = new { id = result.Id, senderId = result.SenderId, receiverId = result.ReceiverId, content = result.Content, createdAt = result.CreatedAt };

        await Clients.Group(UserGroup(receiverId)).SendAsync("pv_message", payload);
        await Clients.Group(UserGroup(senderId)).SendAsync("pv_message", payload);
    }

    private static Guid GetUserId(HubCallerContext ctx)
    {
        var sub = ctx.User?.FindFirst("sub")?.Value 
                  ?? ctx.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(sub)) throw new HubException("Unauthenticated");
        return Guid.Parse(sub);
    }
}
