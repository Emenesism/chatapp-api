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
        var user = await _handler.Handle(command);

        return Ok(new
        {
            user.Id,
            user.Name
        });
    }
}
