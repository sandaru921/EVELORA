using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<Message> Messages { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Message>()
                .Property(m => m.Id)
                .UseIdentityAlwaysColumn();

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .UseIdentityAlwaysColumn();

            modelBuilder.Entity<Jobs>()
                .Property(j => j.Id)
                .UseIdentityAlwaysColumn();
            
            //newly added
            modelBuilder.Entity<Permission>()
                .Property(p => p.Id)
                .UseIdentityAlwaysColumn();

            // Configure many-to-many relationship
            modelBuilder.Entity<UserPermission>()
                .HasKey(up => new { up.UserId, up.PermissionId });

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.User)
                .WithMany(u => u.UserPermissions)
                .HasForeignKey(up => up.UserId);

            modelBuilder.Entity<UserPermission>()
                .HasOne(up => up.Permission)
                .WithMany(p => p.UserPermissions)
                .HasForeignKey(up => up.PermissionId);
            
            // Seed data
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "EditQuiz" },
                new Permission { Id = 2, Name = "DeleteQuiz" },
                new Permission { Id = 3, Name = "CreateQuestion" },
                new Permission { Id = 4, Name = "ViewResults" }
            );
        }
    }

    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Timestamp { get; set; }
    }
}
