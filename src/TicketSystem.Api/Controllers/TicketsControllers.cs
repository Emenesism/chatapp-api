using Microsoft.AspNetCore.Mvc;
using TicketSystem.Api.Contracts.Requests;
using TicketSystem.Api.Contracts.Responses;
using TicketSystem.Application.Abstractions.Repositories;
using TicketSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TicketSystem.Application.Common.Exceptions;


[ApiController]
[Route("ticket")]
public class TicketController(ITicketRepository ticketRepository) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<TicketResponse>> CreateTicket([FromBody] CreateTicketDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var ticket = new Ticket(dto.Title, userId);

        await ticketRepository.CreateTicket(ticket);

        return Ok(new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            CreatedAt = ticket.CreatedAt,
            IsSolved = ticket.Solved
        });
    }

    [HttpGet("user")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<List<TicketResponse>>> GetAllUserTicket()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var ticketsFromDb = await ticketRepository.GetAllUserTicket(userId);

        var result = ticketsFromDb.Select(s => new TicketResponse
        {
            Title = s.Title,
            Id = s.Id,
            CreatedAt = s.CreatedAt,
            IsSolved = s.Solved
        }).ToList();

        return Ok(result);
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<TicketResponse>>> GetAllAdminTicket()
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var ticketsFromDb = await ticketRepository.GetAllAdminTicket(adminId);

        var result = ticketsFromDb.Select(s => new TicketResponse
        {
            Title = s.Title,
            Id = s.Id,
            CreatedAt = s.CreatedAt,
            IsSolved = s.Solved
        }).ToList();

        return Ok(result);
    }

    [HttpPost("solve")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SolveTheTicket([FromBody] TicketIdDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var status = await ticketRepository.ChangeStatusOfSolveToTrue(dto.TicketId, adminId);

        if (!status)
        {
            throw new NotFoundException("The Message Is Not Found Or You Delete Another Person Message");

            // return NotFound(new
            // {
            //     message = "The Ticket You Want To Solve Not Found Or You Are Not Assigned Admin For This Ticket"
            // });
        }

        return Ok(new
        {
            message = "Done"
        });
    }

    [HttpPost("assign")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> AssignAdmin([FromBody] TicketIdDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var status = await ticketRepository.AssignTicketToAdmin(dto.TicketId, adminId);
        if (!status)
        {
            throw new NotFoundException("The Ticket That You Want To Assign Not Found Or Assinged In The Past");

            // return NotFound(new
            // {
            //     message = "The Ticket That You Want To Assign Not Found Or Assinged In The Past"
            // });
        }

        return Ok(new
        {
            message = "Done"
        });

    }
    [HttpGet("not-assign")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<TicketResponse>>> GetAllNotAssignedTicket()
    {
        var tickets = await ticketRepository.GetAllNotAssingedTicket();

        var result = tickets.Select(s => new TicketResponse
        {
            Id = s.Id,
            Title = s.Title,
            CreatedAt = s.CreatedAt,
            IsSolved = s.Solved
        }).ToList();

        return Ok(result);
    }
}
