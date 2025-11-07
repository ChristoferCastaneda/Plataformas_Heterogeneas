using Microsoft.EntityFrameworkCore;
using Comprehension.Models;

namespace Comprehension.Data
{
    public class ComprehensionContext : DbContext
    {
        public ComprehensionContext(DbContextOptions<ComprehensionContext> options)
            : base(options)
        {
        }

        public DbSet<Reminder> Reminder { get; set; } = default!;
        public DbSet<Event> Event { get; set; } = default!;
        public DbSet<Note> Note { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Session> Sessions { get; set; } = default!;
        public DbSet<ResourcePermission> ResourcePermissions { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Session -> User
            modelBuilder.Entity<Session>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Note -> User
            modelBuilder.Entity<Note>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Reminder -> User
            modelBuilder.Entity<Reminder>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Event -> User
            modelBuilder.Entity<Event>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ResourcePermission -> SharedWithUser
            modelBuilder.Entity<ResourcePermission>()
                .HasOne(rp => rp.SharedWithUser)
                .WithMany()
                .HasForeignKey(rp => rp.SharedWithUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ResourcePermission -> Owner
            modelBuilder.Entity<ResourcePermission>()
                .HasOne(rp => rp.Owner)
                .WithMany()
                .HasForeignKey(rp => rp.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice para búsquedas de permisos
            modelBuilder.Entity<ResourcePermission>()
                .HasIndex(rp => new { rp.ResourceId, rp.ResourceType, rp.SharedWithUserId });
        }
    }
}
