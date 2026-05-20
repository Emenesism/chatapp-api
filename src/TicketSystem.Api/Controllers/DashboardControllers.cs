
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Api.Contracts.Responses;
using TicketSystem.Application.Abstractions.Repositories;

namespace TicketSystem.Api.Controllers;


[ApiController]
[Route("dashboard")]
[Authorize(Roles = "SuperAdmin")]
public class DashboardController(ISessionRepo sessionRepo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<AllSessionResponse>>> GetAllSession()
    {
        var session = await sessionRepo.GetActiveSessionForDashboard();

        var result = session.Select(s => new AllSessionResponse
        {
            Id = s.Id,
            UserAgent = s.UserAgent,
            IpAddress = s.IpAddress,
            CreatedAt = s.CreatedAt,
            LastTimeUsed = s.LastUsageAt,
            ExpiresAt = s.ExpiresAt,
            AdminId = s.AdminId,
            UserId = s.UserId,
            IsAdmin = s.IsAdmin
        }).ToList();

        return Ok(result);
    }
}
