using Microsoft.AspNetCore.Mvc;
using TicketSystem.Api.Contracts.Requests;
using TicketSystem.Api.Contracts.Responses;
using TicketSystem.Application.Abstractions.Repositories;
using TicketSystem.Application.Common.Interface;
using TicketSystem.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TicketSystem.Application.Common.Exceptions;
using System.Text.Json;
using System.Collections.Generic;

namespace TicketSystem.Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    IUserRepository userRepository,
    IAdminRepository adminRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService,
    IRefreshTokenGenerator refreshTokenGenerator,
    ISessionRepo sessionRepo,
    IRefreshTokenHasher refreshTokenHasher) : ControllerBase
{
    [HttpPost("user/login")]
    public async Task<ActionResult<CreateUserReponse>> LoginOrRegisterUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var createdRefreshToken = refreshTokenGenerator.GenerateRefreshToken();
        var createdRefreshTokenHash = refreshTokenHasher.Hash(createdRefreshToken);

        var userFromDb = await userRepository.GetUserByUsername(dto.Username);

        if (userFromDb is null)
        {
            var passwordHash = passwordHasher.Hash(dto.Password);
            Console.WriteLine(passwordHash);
            var newUser = new User(dto.Name, dto.Username, passwordHash);

            await userRepository.CreateUser(newUser);

            var createdToken = jwtTokenService.GenerateUserToken(newUser);

            await CreateUserSession(createdRefreshTokenHash, newUser.Id, userAgent, ipAddress);

            SetRefreshTokenInCookie(createdRefreshToken);

            return Ok(new CreateUserReponse
            {
                Id = newUser.Id,
                Name = newUser.Name,
                Username = newUser.Username,
                Token = createdToken,
                CreatedAt = newUser.CreatedAt
            });
        }

        var isValidPassword = passwordHasher.Verify(userFromDb.PasswordHash, dto.Password);

        if (!isValidPassword)
        {
            throw new UnauthorizedException("Invalid Username Or Password.");
            // return Unauthorized(new { Message = "Invalid Username Or Password." });
        }

        var token = jwtTokenService.GenerateUserToken(userFromDb);

        await CreateUserSession(createdRefreshTokenHash, userFromDb.Id, userAgent, ipAddress);

        SetRefreshTokenInCookie(createdRefreshToken);

        return Ok(new CreateUserReponse
        {
            Id = userFromDb.Id,
            Name = userFromDb.Name,
            Username = userFromDb.Username,
            Token = token,
            CreatedAt = userFromDb.CreatedAt
        });
    }

    [HttpPost("admin/login")]
    public async Task<ActionResult<CreateAdminReponse>> LoginAdmin([FromBody] LoginAdminDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var createdRefreshToken = refreshTokenGenerator.GenerateRefreshToken();
        var createdRefreshTokenHash = refreshTokenHasher.Hash(createdRefreshToken);

        var adminFromDb = await adminRepository.GetAdminByUsername(dto.Username);

        if (adminFromDb is null)
        {
            throw new UnauthorizedException("Password Or Username Is Wrong");
        }

        var isValid = passwordHasher.Verify(adminFromDb.PasswordHash, dto.Password);

        if (!isValid)
        {
            throw new UnauthorizedException("Password Or Username Is Wrong.");
            // return Unauthorized(new { Message = "Password Or Username Is Wrong." });
        }

        var token = jwtTokenService.GenerateAdminToken(adminFromDb);


        await CreateAdminSession(createdRefreshTokenHash, adminFromDb.Id, userAgent, ipAddress);
        SetRefreshTokenInCookie(createdRefreshToken);

        return Ok(new CreateAdminReponse
        {
            Id = adminFromDb.Id,
            Name = adminFromDb.Name,
            Username = adminFromDb.Username,
            Token = token,
            CreatedAt = adminFromDb.CreatedAt,
            Roles = adminFromDb.GetRoles(),
            IsSuperAdmin = adminFromDb.IsSuperAdmin
        });
    }

    [HttpPost("admin/refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RenewAdminAccessToken()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedException("Refresh Token Is Missing.");
            // return Unauthorized(new
            // {
            //     message = "Refresh Token Is Missing."
            // });
        }

        var refreshTokenHash = refreshTokenHasher.Hash(refreshToken);

        var reuseStatus = await RevokeAllSessionIfReuseDetected(refreshTokenHash);

        if (reuseStatus)
        {
            return Unauthorized();
        }

        var session = await sessionRepo.GetSessionByHashToken(refreshTokenHash);

        if (session is null)
        {
            throw new UnauthorizedException("Refresh Token Is Not Valid.");
            // return Unauthorized(new
            // {
            // message = "Refresh Token Is Not Valid."
            // });
        }

        if (session.AdminId is null || !session.IsAdmin)
        {
            throw new UnauthorizedException("You Are In Wrong Place Dude");
            // return Unauthorized(new
            // {
            //     message = "You Are In Wrong Place Dude."
            // });
        }

        var adminFromDb = await adminRepository.GetAdminById(session.AdminId.Value);

        if (adminFromDb is null)
        {
            throw new UnauthorizedException("You Are In Wrong Place Dude");
            // return Unauthorized(new
            // {
            //     message = "You Are In Wrong Place Dude."
            // });
        }


        var revokeStatus = await sessionRepo.RevokeSession(session.Id);

        if (!revokeStatus)
        {
            throw new UnauthorizedException("You Are In Wrong Place Dude");
            // return Unauthorized(new
            // {
            //     message = "Failed To Revoke Old Session."
            // });
        }

        var newAccessToken = jwtTokenService.GenerateAdminToken(adminFromDb);
        var newRefreshToken = refreshTokenGenerator.GenerateRefreshToken();
        var newRefreshTokenHash = refreshTokenHasher.Hash(newRefreshToken);

        await CreateAdminSession(newRefreshTokenHash, session.AdminId.Value, userAgent, ipAddress);

        SetRefreshTokenInCookie(newRefreshToken);

        return Ok(new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
        });

    }

    [HttpPost("user/refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RenewUserAccessToken()
    {
        var userAgent = Request.Headers.UserAgent.ToString();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new UnauthorizedException("Refresh Token Is Missing.");
            // return Unauthorized(new
            // {
            //     message = "Refresh Token Is Missing."
            // });
        }

        var refreshTokenHash = refreshTokenHasher.Hash(refreshToken);

        var status = await RevokeAllSessionIfReuseDetected(refreshTokenHash);

        if (status)
        {
            throw new UnauthorizedException("You Are In Wrong Place Dude");
            // return Unauthorized();
        }

        var session = await sessionRepo.GetSessionByHashToken(refreshTokenHash);

        if (session is null)
        {
            throw new UnauthorizedException("Refresh Token Is Not Valid.");
            // return Unauthorized(new
            // {
            //     message = "Refresh Token Is Not Valid."
            // });
        }

        if (session.UserId is null || session.IsAdmin)
        {
            throw new UnauthorizedException("You Are In Wrong Place Dude");
            // return Unauthorized(new
            // {
            //     message = "You Are In Wrong Place Dude."
            // });
        }

        var userFromDb = await userRepository.GetUserById(session.UserId.Value);

        if (userFromDb is null)
        {
            throw new UnauthorizedException("You Are In Wrong Place Dude");
            // return Unauthorized(new
            // {
            //     message = "You Are In Wrong Place Dude."
            // });
        }

        var revokeStatus = await sessionRepo.RevokeSession(session.Id);

        if (!revokeStatus)
        {
            throw new UnauthorizedException("Faild To Revoke Old Session.");
            // return Unauthorized(new
            // {
            //     message = "Failed To Revoke Old Session."
            // });
        }

        var newAccessToken = jwtTokenService.GenerateUserToken(userFromDb);
        var newRefreshToken = refreshTokenGenerator.GenerateRefreshToken();
        var newRefreshTokenHash = refreshTokenHasher.Hash(newRefreshToken);

        await CreateUserSession(newRefreshTokenHash, session.UserId.Value, userAgent, ipAddress);

        SetRefreshTokenInCookie(newRefreshToken);

        return Ok(new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
        });

    }



    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            DeleteRefreshTokenInCookie();
            return NoContent();
        }
        var refreshTokenHash = refreshTokenHasher.Hash(refreshToken);

        var sessionFromDb = await sessionRepo.GetSessionByHashToken(refreshTokenHash);
        if (sessionFromDb is null)
        {
            DeleteRefreshTokenInCookie();
            return NoContent();
        }

        await sessionRepo.RevokeSession(sessionFromDb.Id);

        DeleteRefreshTokenInCookie();
        return NoContent();

    }

    [HttpPost("admin/revoke-all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RevokeAllAdminSessionsController()
    {
        var adminId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await sessionRepo.RevokeAllAdminSessions(adminId);

        DeleteRefreshTokenInCookie();

        return NoContent();

    }

    [HttpPost("user/revoke-all")]
    [Authorize(Roles = "User")]
    public async Task<ActionResult> RevokeAllUserSessionsController()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await sessionRepo.RevokeAllUserSessions(userId);

        DeleteRefreshTokenInCookie();

        return NoContent();

    }

    [HttpPost("admin/create")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult> CreateAdmin(CreateAdminDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var passwordHash = passwordHasher.Hash(dto.Password);


        var admin = new Admin(
            dto.Name,
            dto.Username,
            passwordHash,
            dto.IsSuperAdmin
        );

        admin.StoreRoles(dto.Roles);

        await adminRepository.CreateAdmin(admin);

        return Ok(new { message = "ok done" });
    }




    private async Task CreateUserSession(string refreshTokenHash, Guid userId, string userAgent, string? ipAddress)
    {
        var session = new Session(
            refreshTokenHash,
            null,
            userId,
            false,
            DateTime.UtcNow.AddDays(30),
            userAgent,
            ipAddress
        );

        await sessionRepo.CreateSession(session);
    }


    private async Task CreateAdminSession(string refreshTokenHash, Guid adminId, string userAgent, string? ipAddress)
    {
        var session = new Session(
            refreshTokenHash,
            adminId,
            null,
            true,
            DateTime.UtcNow.AddDays(30),
            userAgent,
            ipAddress
        );

        await sessionRepo.CreateSession(session);
    }

    private async Task<bool> RevokeAllSessionIfReuseDetected(string refreshTokenHash)
    {
        var session = await sessionRepo.ReuseDetector(refreshTokenHash);

        if (session is not null)
        {
            await (session.IsAdmin ? sessionRepo.RevokeAllAdminSessions(session.AdminId!.Value) : sessionRepo.RevokeAllUserSessions(session.UserId!.Value));
            DeleteRefreshTokenInCookie();
            return true;
        }

        return false;

    }

    private void SetRefreshTokenInCookie(string refreshToken)
    {
        Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            SameSite = SameSiteMode.Lax,
            HttpOnly = true,
            Secure = false,
            Path = "/auth",
            Expires = DateTime.UtcNow.AddDays(30)
        });

    }

    private void DeleteRefreshTokenInCookie()
    {
        Response.Cookies.Delete("refreshToken", new CookieOptions
        {
            Path = "/auth",
            Secure = false,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,

        });
    }




}
