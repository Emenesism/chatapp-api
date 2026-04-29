using ChatApp.Application.Messages.PV;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.WebApi.Controllers.Messages;

[ApiController]
[Authorize]
[Route("messages")]
public class MessagesController : ControllerBase
{
    private readonly SendPvMessageHandler _sendPvMessage;
    private readonly GetPvHistoryHandler _getPvHistory;

    public MessagesController(
        SendPvMessageHandler sendPvMessage, 
        GetPvHistoryHandler getPvHistory)
    {
        _sendPvMessage = sendPvMessage;
        _getPvHistory = getPvHistory;
    }

    [HttpPost("pv")]
    public async Task<IActionResult> SendPv([FromBody] SendPvMessageRequest request)
    {
        var senderId = GetUserId();
        var result = await _sendPvMessage.Handle(senderId, request);
        return Ok(result);
    }

    [HttpGet("pv/{otherUserId:guid}")]
    public async Task<IActionResult> GetPvHistory(
        Guid otherUserId,
        [FromQuery] DateTime? before = null,
        [FromQuery] int take = 50)
    {
        var userId = GetUserId();
        var result = await _getPvHistory.Handle(userId, new GetPvHistoryQuery(otherUserId, before, take));
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
