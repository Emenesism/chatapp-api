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

builder.Services.AddDbContext<ChatDBContext>(options =>
    options.UseNpgsql(
       conn
    ));

builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<LoginOrRegisterHandler>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
