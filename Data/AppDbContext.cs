using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Models;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
            // UserProfile configuration with new nested structure
            modelBuilder.Entity<UserProfile>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
                entity.Property<int>("UserIdInt").IsRequired().HasColumnName("UserIdInt");
                entity.HasOne<User>().WithOne().HasForeignKey<UserProfile>("UserIdInt").OnDelete(DeleteBehavior.Cascade);

                // Configure owned types for nested objects with PostgreSQL arrays
                entity.OwnsOne(e => e.Education)
                    .Property(p => p.Text)
                    .HasColumnName("EducationText");

                entity.OwnsOne(e => e.Education)
                    .Property(p => p.Evidence)
                    .HasColumnName("EducationEvidence")
                    .HasConversion(
                        v => v ?? Array.Empty<string>(), // Convert null to empty array
                        v => v,                         // No conversion needed for return
                        new ValueComparer<string[]>(    // Ensure proper equality comparison
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToArray()
                        )
                    );

                entity.OwnsOne(e => e.Education)
                    .Property(p => p.Status)
                    .HasColumnName("EducationStatus");

                entity.OwnsOne(e => e.WorkExperience)
                    .Property(p => p.Text)
                    .HasColumnName("WorkExperienceText");

                entity.OwnsOne(e => e.WorkExperience)
                    .Property(p => p.Evidence)
                    .HasColumnName("WorkExperienceEvidence")
                    .HasConversion(
                        v => v ?? Array.Empty<string>(),
                        v => v,
                        new ValueComparer<string[]>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToArray()
                        )
                    );

                entity.OwnsOne(e => e.WorkExperience)
                    .Property(p => p.Status)
                    .HasColumnName("WorkExperienceStatus");

                entity.OwnsOne(e => e.Skills)
                    .Property(p => p.Text)
                    .HasColumnName("SkillsText");

                entity.OwnsOne(e => e.Skills)
                    .Property(p => p.Evidence)
                    .HasColumnName("SkillsEvidence")
                    .HasConversion(
                        v => v ?? Array.Empty<string>(),
                        v => v,
                        new ValueComparer<string[]>(
                            (c1, c2) => c1.SequenceEqual(c2),
                            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                            c => c.ToArray()
                        )
                    );

                entity.OwnsOne(e => e.Skills)
                    .Property(p => p.Status)
                    .HasColumnName("SkillsStatus");
            });

            // User configuration (unchanged)
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // Job configuration (unchanged)
            modelBuilder.Entity<Job>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            // Message configuration (unchanged)
            modelBuilder.Entity<Message>()
                .Property(m => m.Id)
                .UseIdentityAlwaysColumn();

            // User configuration with identity (unchanged)
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .UseIdentityAlwaysColumn();

            // Job configuration with identity (unchanged)
            modelBuilder.Entity<Job>()
                .Property(j => j.Id)
                .UseIdentityAlwaysColumn();

            // Permission configuration with identity (unchanged)
            modelBuilder.Entity<Permission>()
                .Property(p => p.Id)
                .UseIdentityAlwaysColumn();

            // Many-to-many relationship configuration (unchanged)
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

            // Seed permissions with DisplayName (unchanged)
            var permissions = new[]
            {
                new Permission { Id = 1, Name = "EditQuiz", DisplayName = GenerateDisplayName("EditQuiz") },
                new Permission { Id = 2, Name = "DeleteQuiz", DisplayName = GenerateDisplayName("DeleteQuiz") },
                new Permission { Id = 3, Name = "CreateQuestion", DisplayName = GenerateDisplayName("CreateQuestion") },
                new Permission { Id = 4, Name = "ViewResults", DisplayName = GenerateDisplayName("ViewResults") },
                new Permission { Id = 5, Name = "Admin", DisplayName = GenerateDisplayName("Admin") }
            };

            modelBuilder.Entity<Permission>().HasData(permissions);

            // QuizResult configuration (unchanged)
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

        // Helper: convert camelCase to spaced words (unchanged)
        private string GenerateDisplayName(string name)
        {
            return Regex.Replace(name, "(\\B[A-Z])", " $1");
        }
    }
}