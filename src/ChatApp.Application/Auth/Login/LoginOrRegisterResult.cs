namespace ChatApp.Application.Auth.Login;

public record LoginOrRegisterResult(
    Guid UserId,
    string Name,
    string PhoneNumber,
    string Token
);
