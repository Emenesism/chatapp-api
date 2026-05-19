using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Application.Abstractions.Repositories;
using TicketSystem.Application.Dtos.Notification;

namespace TicketSystem.Api.Controllers;

[ApiController]
[Route("notification")]
[Authorize]
public class NotificationContrller(INotificationRepository notificationRepo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<GetUserNotificationResponse>>> GetUserNotification()
    {

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        var userNotification = await notificationRepo.GetUserNotification(userId);

        var result = userNotification.Select(s => new GetUserNotificationResponse
        {
            Id = s.Id,
            Title = s.Title,
            Body = s.Body,
            CreatedAt = s.CreatedAt
        }).ToList();

        return Ok(result);
    }
}
