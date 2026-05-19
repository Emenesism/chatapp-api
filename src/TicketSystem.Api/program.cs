using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TicketSystem.Application.Abstractions.Repositories;
using TicketSystem.Application.Common.Interface;
using TicketSystem.Infrastructure.Persistance.Configuration;
using TicketSystem.Infrastructure.Persistance.Repositories;
using TicketSystem.Infrastructure.Security;
using TicketSystem.Api.Middleware;
using TicketSystem.Api.Hubs;
using TicketSystem.Application.Services;
using TicketSystem.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var conn = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(conn);
});

builder.Services.AddScoped<IPasswordHasher, PassowordHasherService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUserRepository, UserRepo>();
builder.Services.AddScoped<IAdminRepository, AdminRepo>();
builder.Services.AddScoped<ITicketRepository, TicketRepo>();
builder.Services.AddScoped<ITicketMessageRepository, TicketMessageRepo>();
builder.Services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
builder.Services.AddScoped<ISessionRepo, SessionRepo>();
builder.Services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationSender, SignalRNotificationSender>();

var storagePath = builder.Configuration.GetValue<string>("FileStorage:BasePath") ?? "uploads";
if (!Path.IsPathRooted(storagePath))
{
    storagePath = Path.Combine(builder.Environment.ContentRootPath, storagePath);
}
builder.Services.AddSingleton<IFileStorageService>(new FileStorageService(storagePath));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://127.0.0.1:8080",
                "http://localhost:8080"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var jwt = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)
            ),

            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hub/notifications"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddSignalR();

builder.Services.AddAuthorization();

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseRouting();

app.UseCors("Frontend");

app.UseAuthentication();


app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/hub/notifications");

await app.ApplyAllMigrateAsync();

app.Run();
