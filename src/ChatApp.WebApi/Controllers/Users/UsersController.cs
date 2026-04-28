using Microsoft.AspNetCore.Mvc;
using ChatApp.Application.Auth.Login;

namespace ChatApp.WebApi.Controllers.Users;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly LoginOrRegisterHandler _handler;

    public UsersController(LoginOrRegisterHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginOrRegisterCommand command)
    {
        var result = await _handler.Handle(command);

        return Ok(new
        {
            result.User.Id,
            result.User.Name,
            token = result.Token
        });
    }
}
