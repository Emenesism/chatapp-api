using Microsoft.EntityFrameworkCore;
using ChatApp.Domain.Entities;

namespace ChatApp.Infrastructure.Persistence;

public class ChatDBContext : DbContext
{
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<User> Users => Set<User>();
    public DbSet<GroupChat> GroupChats => Set<GroupChat>();
    public DbSet<GroupMembership> GroupMemberships => Set<GroupMembership>();
    public DbSet<GroupMessage> GroupMessages => Set<GroupMessage>();

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

        builder.Entity<GroupChat>()
            .HasOne(g => g.Creator)
            .WithMany()
            .HasForeignKey(g => g.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupChat>()
            .HasIndex(g => g.Username)
            .IsUnique();

        builder.Entity<GroupMembership>()
            .HasKey(gm => new { gm.GroupId, gm.UserId });

        builder.Entity<GroupMembership>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.Members)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupMembership>()
            .HasOne(gm => gm.User)
            .WithMany()
            .HasForeignKey(gm => gm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupMessage>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.Messages)
            .HasForeignKey(gm => gm.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<GroupMessage>()
            .HasOne(gm => gm.Sender)
            .WithMany()
            .HasForeignKey(gm => gm.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<GroupMessage>()
            .HasIndex(gm => new { gm.GroupId, gm.CreatedAt });
    }

}
