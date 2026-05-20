using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TicketSystem.Api.Contracts.Common;
using TicketSystem.Api.Contracts.Requests;
using TicketSystem.Api.Contracts.Responses;
using TicketSystem.Application.Abstractions.Repositories;
using TicketSystem.Application.Common.Exceptions;

namespace TicketSystem.Api.Controllers;


[ApiController]
[Route("users")]
[Authorize(Roles = "UserAccess")]
public sealed class UserController(IUserRepository userRepository) : ControllerBase
{
    [HttpGet("all")]
    public async Task<ActionResult<PaginatedResponse<GetUserResponse>>> GetAllUsers([FromQuery] PaginationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        var usersFromDb = await userRepository.GetUsersAsync(dto.Page, dto.Limit);

        var totalCount = await userRepository.GetTotalUserCount();

        var totalPage = (int)Math.Ceiling(totalCount / (double)dto.Limit);

        var result = usersFromDb.Select(s => new GetUserResponse
        {
            Name = s.Name,
            Username = s.Username,
            CreatedAt = s.CreatedAt
        }).ToList();

        return Ok(new PaginatedResponse<GetUserResponse>
        {
            Items = result,
            TotalCount = totalCount,
            TotalPages = totalPage,
            Page = dto.Page,
            Limit = dto.Limit
        });

    }

    [HttpGet("by-username")]
    public async Task<ActionResult<GetUserResponse>> GetUserViaUsername([FromQuery] string username)
    {
        var userFromDb = await userRepository.GetUserByUsername(username);

        if (userFromDb is null)
        {
            throw new NotFoundException($"User With Username {username} Not Found");
            // return NotFound(new
            // {
            //     message = $"User With Username {username} Not Found"
            // });
        }

        return Ok(new GetUserResponse
        {
            Name = userFromDb.Name,
            Username = userFromDb.Username,
            CreatedAt = userFromDb.CreatedAt

        });
    }

    [HttpGet("by-name")]
    public async Task<ActionResult<GetUserResponse>> GetUserViaName([FromQuery] string name)
    {
        var userFromDb = await userRepository.GetUserByName(name);

        if (userFromDb is null)
        {
            throw new NotFoundException($"User With name {name} Not Found");

            // return NotFound(new
            // {
            //     message = $"User With Name {name} Not Found"
            // });
        }

        return Ok(new GetUserResponse
        {
            Name = userFromDb.Name,
            Username = userFromDb.Username,
            CreatedAt = userFromDb.CreatedAt
        });
    }

    [HttpGet("before-date")]
    public async Task<ActionResult<PaginatedResponse<GetUserResponse>>> GetUserBeforeCertainDate([FromQuery] GetUsersFilterDto dto)
    {

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        var utcDate = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc);

        var usersFromDb = await userRepository.GetAllUserBeforeCertianDate(utcDate, dto.Page, dto.Limit);



        if (!usersFromDb.Any())
        {
            throw new NotFoundException($"No User Exist Before date {dto.Date}");

            // return NotFound(new
            // {
            //     Message = $"No User Exist Before date {date}"
            // });
        }

        var totalCount = await userRepository.GetTotalUserCountBeforeDate(dto.Date);
        var totalPage = (int)Math.Ceiling(totalCount / (double)dto.Limit);

        var result = usersFromDb.Select(s => new GetUserResponse
        {
            Name = s.Name,
            Username = s.Username,
            CreatedAt = s.CreatedAt
        }).ToList();

        return Ok(new PaginatedResponse<GetUserResponse>
        {
            Items = result,
            Page = dto.Page,
            Limit = dto.Limit,
            TotalCount = totalCount,
            TotalPages = totalPage
        });

    }

    [HttpGet("after-date")]
    public async Task<ActionResult<PaginatedResponse<GetUserResponse>>> GetUserAfterCertainDate([FromQuery] GetUsersFilterDto dto)
    {

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var utcDate = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc);
        var usersFromDb = await userRepository.GetAllUserAfterCertianDate(utcDate, dto.Page, dto.Limit);

        if (!usersFromDb.Any())
        {
            throw new NotFoundException($"No User Exist After date {dto.Date}");
            // return NotFound(new
            // {
            //     Message = $"No User Exist After date {date}"
            // });
        }

        var totalCount = await userRepository.GetTotalUserCountAfterDate(dto.Date);
        var totalPage = (int)Math.Ceiling(totalCount / (double)dto.Limit);

        var result = usersFromDb.Select(s => new GetUserResponse
        {
            Name = s.Name,
            Username = s.Username,
            CreatedAt = s.CreatedAt
        }).ToList();

        return Ok(new PaginatedResponse<GetUserResponse>
        {
            Items = result,
            Page = dto.Page,
            Limit = dto.Limit,
            TotalCount = totalCount,
            TotalPages = totalPage
        });

    }

}
