using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Application.Abstractions.Security;
using ChatApp.Application.Common.Validator;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Auth.Login;

public class LoginOrRegisterHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtProvider _jwt;

    public LoginOrRegisterHandler(IUserRepository users, IPasswordHasher hasher, IJwtProvider jwt)
    {
        _users = users;
        _hasher = hasher;
        _jwt = jwt;
    }

    public async Task<LoginOrRegisterResult> Handle(LoginOrRegisterCommand command)
    {
        var phone = PhoneValidator.NormalizePhone(command.Phone);

        var user = await _users.GetByPhoneAsync(phone);

        if (user is null)
        {
            var hash = _hasher.Hash(command.Password);
            user = new User(command.Name, phone, hash);
            await _users.AddAsync(user);
        }
        else
        {
            var isValid = _hasher.Verify(command.Password, user.PasswordHash ?? string.Empty);
            if (!isValid)
            {
                throw new InvalidOperationException("Invalid credentials");
            }
        }
        var token = _jwt.GenerateToken(user.Id, user.PhoneNumber);

        return new LoginOrRegisterResult(user, token);
    }
}
