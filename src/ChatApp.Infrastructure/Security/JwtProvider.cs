using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChatApp.Application.Abstractions.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChatApp.Infrastructure.Security;

public class JwtProvider : IJwtProvider
{
    private readonly string _key;

    public JwtProvider(IConfiguration config)
    {
        _key = config["Jwt:Key"] ?? throw new InvalidOperationException(
            "JWT key is missing. Set Jwt:Key in configuration (for .env use JWT__KEY).");

        if (string.IsNullOrWhiteSpace(_key))
        {
            throw new InvalidOperationException(
                "JWT key is empty. Set Jwt:Key in configuration (for .env use JWT__KEY).");
        }
    }

    public string GenerateToken(Guid userId, string phone)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("phone", phone)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
