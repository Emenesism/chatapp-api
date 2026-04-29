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

        builder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.Messages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Message>().HasIndex(m => new { m.SenderId, m.ReceiverId, m.CreatedAt });
        builder.Entity<Message>().HasIndex(m => new { m.ReceiverId, m.SenderId, m.CreatedAt });
    }

}
