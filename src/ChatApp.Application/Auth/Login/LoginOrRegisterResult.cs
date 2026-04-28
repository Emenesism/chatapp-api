using ChatApp.Domain.Entities;

namespace ChatApp.Application.Auth.Login;

public record LoginOrRegisterResult(User User, string Token);
