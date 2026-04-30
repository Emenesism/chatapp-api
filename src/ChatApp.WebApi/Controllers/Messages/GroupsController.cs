using ChatApp.Application.Messages.Groups;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers.Messages;

[ApiController]
[Authorize]
[Route("groups")]
public class GroupsController : ControllerBase
{
    private readonly CreateGroupChatHandler _createGroup;
    private readonly JoinGroupByUsernameHandler _joinGroup;
    private readonly GetGroupHistoryHandler _getGroupHistory;
    private readonly GetMyGroupsHandler _getMyGroups;

    public GroupsController(
        CreateGroupChatHandler createGroup,
        JoinGroupByUsernameHandler joinGroup,
        GetGroupHistoryHandler getGroupHistory,
        GetMyGroupsHandler getMyGroups)
    {
        _createGroup = createGroup;
        _joinGroup = joinGroup;
        _getGroupHistory = getGroupHistory;
        _getMyGroups = getMyGroups;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyGroups()
    {
        var userId = GetUserId();
        var result = await _getMyGroups.Handle(userId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGroupChatRequest request)
    {
        var userId = GetUserId();
        var result = await _createGroup.Handle(userId, request);
        return Ok(result);
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] JoinGroupByUsernameRequest request)
    {
        var userId = GetUserId();
        var result = await _joinGroup.Handle(userId, request);
        return Ok(result);
    }

    [HttpGet("{groupUsername}")]
    public async Task<IActionResult> GetHistory(
        string groupUsername,
        [FromQuery] DateTime? before = null,
        [FromQuery] int take = 50)
    {
        var userId = GetUserId();
        var result = await _getGroupHistory.Handle(userId, new GetGroupHistoryQuery(groupUsername, before, take));
        return Ok(result);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirst("sub")?.Value
                  ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(sub))
            throw new UnauthorizedAccessException("Unauthenticated");

        return Guid.Parse(sub);
    }
}
