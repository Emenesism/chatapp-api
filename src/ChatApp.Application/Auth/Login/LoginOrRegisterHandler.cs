using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Application.Abstractions.Security;
using ChatApp.Application.Common.Validator;
using ChatApp.Domain.Entities;

namespace ChatApp.Application.Auth.Login;

public class LoginOrRegisterHandler
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;

    public LoginOrRegisterHandler(IUserRepository users, IPasswordHasher hasher)
    {
        _users = users;
        _hasher = hasher;
    }

    public async Task<User> Handle(LoginOrRegisterCommand command)
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

        return user;
    }
}
