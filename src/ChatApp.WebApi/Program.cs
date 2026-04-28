using Microsoft.EntityFrameworkCore;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Infrastructure.Persistence.Repositories;
using ChatApp.Domain.Entities;
using DotNetEnv;
using ChatApp.Application.Abstractions.Security;
using ChatApp.Infrastructure.Security;
using ChatApp.Application.Auth.Login;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var conn = Environment.GetEnvironmentVariable("DB_CONNECTION");
if (string.IsNullOrWhiteSpace(conn))
{
    throw new InvalidOperationException("DB_CONNECTION is missing. Set it in .env.");
}

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("Jwt:Key is missing. For .env use JWT__KEY.");
}

builder.Services.AddDbContext<ChatDBContext>(options =>
    options.UseNpgsql(
       conn
    ));

builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<LoginOrRegisterHandler>();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
