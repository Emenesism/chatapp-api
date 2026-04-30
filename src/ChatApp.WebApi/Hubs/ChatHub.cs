using ChatApp.Application.Messages.PV;
using ChatApp.Application.Messages.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.WebApi.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly SendPvMessageHandler _sendPvMessage;
    private readonly JoinGroupByUsernameHandler _joinGroup;
    private readonly SendGroupMessageHandler _sendGroupMessage;
    private readonly GetMyGroupsHandler _getMyGroups;

    public ChatHub(
        SendPvMessageHandler sendPvMessage,
        JoinGroupByUsernameHandler joinGroup,
        SendGroupMessageHandler sendGroupMessage,
        GetMyGroupsHandler getMyGroups)
    {
        _sendPvMessage = sendPvMessage;
        _joinGroup = joinGroup;
        _sendGroupMessage = sendGroupMessage;
        _getMyGroups = getMyGroups;
    }

    public static string UserGroup(Guid userId) => $"user:{userId}";
    public static string ChatGroup(string groupUsername) => $"group:{groupUsername.Trim().ToLowerInvariant()}";

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId(Context);
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        
        var userGroups = await _getMyGroups.Handle(userId);
        foreach (var group in userGroups)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ChatGroup(group.Username));
        }

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

    public async Task JoinGroupChat(string groupUsername)
    {
        var userId = GetUserId(Context);
        var result = await _joinGroup.Handle(userId, new JoinGroupByUsernameRequest(groupUsername));
        await Groups.AddToGroupAsync(Context.ConnectionId, ChatGroup(result.GroupUsername));
    }

    public async Task SendGroup(string groupUsername, string content)
    {
        var senderId = GetUserId(Context);
        var result = await _sendGroupMessage.Handle(senderId, new SendGroupMessageRequest(groupUsername, content));
        var payload = new
        {
            id = result.Id,
            groupId = result.GroupId,
            groupUsername = result.GroupUsername,
            senderId = result.SenderId,
            content = result.Content,
            createdAt = result.CreatedAt
        };

        await Clients.Group(ChatGroup(result.GroupUsername)).SendAsync("group_message", payload);
    }

    private static Guid GetUserId(HubCallerContext ctx)
    {
        var sub = ctx.User?.FindFirst("sub")?.Value 
                  ?? ctx.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(sub)) throw new HubException("Unauthenticated");
        return Guid.Parse(sub);
    }
}
