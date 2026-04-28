namespace ChatApp.Application.Abstractions.Security;

public interface IJwtProvider
{
    string GenerateToken(Guid userId, string phone);
}
