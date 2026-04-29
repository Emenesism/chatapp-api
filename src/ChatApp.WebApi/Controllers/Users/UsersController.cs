using Microsoft.AspNetCore.Mvc;
using ChatApp.Application.Auth.Login;
using ChatApp.Application.Users.GetUsers;

namespace ChatApp.WebApi.Controllers.Users;

[ApiController]
[Route("users")]
public class UsersController : ControllerBase
{
    private readonly LoginOrRegisterHandler _loginOrRegister;
    private readonly GetUsersHandler _getUsers;

    public UsersController(LoginOrRegisterHandler loginOrRegister, GetUsersHandler getUsers)
    {
        _loginOrRegister = loginOrRegister;
        _getUsers = getUsers;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginOrRegisterCommand command)
    {
        var result = await _loginOrRegister.Handle(command);

        return Ok(new
        {
            id = result.UserId,
            name = result.Name,
            phoneNumber = result.PhoneNumber,
            token = result.Token
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _getUsers.Handle();
        return Ok(users);
    }
}
