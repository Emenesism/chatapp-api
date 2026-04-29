using Microsoft.EntityFrameworkCore;
using ChatApp.Infrastructure.Persistence;
using ChatApp.Application.Abstractions.Repositories;
using ChatApp.Infrastructure.Persistence.Repositories;
using DotNetEnv;
using ChatApp.Application.Abstractions.Security;
using ChatApp.Infrastructure.Security;
using ChatApp.Application.Auth.Login;
using ChatApp.Application.Messages.PV;
using ChatApp.Application.Users.GetUsers;
using ChatApp.WebApi.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

Env.Load();
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

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
builder.Services.AddScoped<GetUsersHandler>();
builder.Services.AddScoped<SendPvMessageHandler>();
builder.Services.AddScoped<GetPvHistoryHandler>();

builder.Services.AddControllers();
builder.Services.AddSignalR();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/chat"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
