using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Models;
using System.Text.RegularExpressions;



namespace AssessmentPlatform.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
       public DbSet<QuizResult> QuizResults { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property<int>("UserIdInt").IsRequired().HasColumnName("UserIdInt");
                entity.HasOne<User>().WithOne().HasForeignKey<UserProfile>("UserIdInt").OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Message>()
                .Property(m => m.Id)
                .UseIdentityAlwaysColumn();

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .UseIdentityAlwaysColumn();

            modelBuilder.Entity<Job>()
                .Property(j => j.Id)
                .UseIdentityAlwaysColumn();

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

            // Seed permissions with DisplayName
            var permissions = new[]
            {
                new Permission { Id = 1, Name = "EditQuiz", DisplayName = GenerateDisplayName("EditQuiz") },
                new Permission { Id = 2, Name = "DeleteQuiz", DisplayName = GenerateDisplayName("DeleteQuiz") },
                new Permission { Id = 3, Name = "CreateQuestion", DisplayName = GenerateDisplayName("CreateQuestion") },
                new Permission { Id = 4, Name = "ViewResults", DisplayName = GenerateDisplayName("ViewResults") },
                new Permission { Id = 5, Name = "Admin", DisplayName = GenerateDisplayName("Admin") }
            };

            modelBuilder.Entity<Permission>().HasData(permissions);
            
             modelBuilder.Entity<QuizResult>(entity =>
        {
            entity.HasKey(q => q.Id);
            entity.Property(q => q.UserId).HasMaxLength(50);
            entity.Property(q => q.UserIdInt).IsRequired();
            entity.Property(q => q.QuizId).IsRequired();
            entity.Property(q => q.Score).IsRequired();
            entity.Property(q => q.TotalMarks).IsRequired();
            entity.Property(q => q.SubmissionTime).IsRequired();
            entity.Property(q => q.TimeTaken).IsRequired();

            entity.HasKey(e => e.Id);
    entity.Property<int>("UserIdInt").HasColumnName("UserIdInt");

    // FK to Users table
    entity.HasOne<User>()
          .WithMany()
          .HasForeignKey("UserIdInt")
          .OnDelete(DeleteBehavior.Cascade);
        });
        }
        // Helper: convert camelCase to spaced words
        private string GenerateDisplayName(string name)
        {
            return Regex.Replace(name, "(\\B[A-Z])", " $1");
        }
    }

        // public class Message
        // {
        //     public int Id { get; set; }
        //     public string Text { get; set; }
        //     public string Sender { get; set; }
        //     public string Recipient { get; set; }
        //     public string Timestamp { get; set; }
        // }
}