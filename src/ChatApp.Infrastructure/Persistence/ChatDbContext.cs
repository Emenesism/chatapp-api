using Microsoft.EntityFrameworkCore;
using ChatApp.Domain.Entities;

namespace ChatApp.Infrastructure.Persistence;

public class ChatDBContext : DbContext
{
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<User> Users => Set<User>();

    public ChatDBContext(DbContextOptions<ChatDBContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ChatDBContext).Assembly);
    }

}
