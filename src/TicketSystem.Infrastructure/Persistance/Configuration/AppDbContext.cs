using Microsoft.EntityFrameworkCore;
using TicketSystem.Domain.Entities;

namespace TicketSystem.Infrastructure.Persistance.Configuration;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Ticket> Tickets { get; set; }
    public DbSet<TicketMessage> TicketMessages { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Notification> Notifications { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>().HasQueryFilter(s => s.IsDeleted != true);
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();

        // Admin
        modelBuilder.Entity<Admin>().HasQueryFilter(s => s.IsDeleted != true);
        modelBuilder.Entity<Admin>().HasIndex(a => a.Username).IsUnique();

        // Ticket
        modelBuilder.Entity<Ticket>().HasMany(t => t.Messages).WithOne(m => m.Ticket).HasForeignKey(m => m.TicketId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Ticket>().HasOne(t => t.User).WithMany(u => u.Tickets).HasForeignKey(u => u.UserId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Ticket>().HasOne(t => t.Admin).WithMany(u => u.Tickets).HasForeignKey(u => u.AdminId).OnDelete(DeleteBehavior.Restrict);

        // Attachments
        modelBuilder.Entity<TicketMessage>().HasMany(m => m.Attachments).WithOne(a => a.TicketMessage).HasForeignKey(a => a.TicketMessageId).OnDelete(DeleteBehavior.Cascade);


    }
}
