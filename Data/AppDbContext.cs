// AssessmentPlatform.Backend/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Models;
using System.Text.RegularExpressions;

namespace AssessmentPlatform.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<JobQuiz> JobQuizzes { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<UserPermission> UserPermissions { get; set; }

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

            modelBuilder.Entity<Job>()
                .Property(j => j.Id)
                .UseIdentityAlwaysColumn();

            modelBuilder.Entity<Quiz>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.QuizName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.JobCategory).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.QuizLevel).IsRequired().HasMaxLength(50);
                entity.Property(e => e.QuizDuration).IsRequired();
                entity.HasIndex(e => e.QuizName).IsUnique().HasDatabaseName("IX_Quiz_QuizName");
            });

            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.QuestionText).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CodeSnippet).HasMaxLength(4000).IsRequired(false);
                entity.Property(e => e.ImageURL).HasMaxLength(500).IsRequired(false);
                entity.Property(e => e.Marks).IsRequired();
                entity.Property(e => e.CorrectAnswers)
                    .HasConversion(
                        v => v != null && v.Any() ? string.Join(";", v) : string.Empty,
                        v => !string.IsNullOrEmpty(v) ? v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>())
                    .HasColumnType("text")
                    .IsRequired();
                entity.HasOne(e => e.Quiz)
                    .WithMany(e => e.Questions)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                entity.HasIndex(e => e.QuizId).HasDatabaseName("IX_Question_QuizId");
            });

            modelBuilder.Entity<Option>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.Key).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Value).IsRequired().HasMaxLength(500);
                entity.HasOne(e => e.Question)
                    .WithMany(e => e.Options)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                entity.HasIndex(e => new { e.QuestionId, e.Key })
                    .IsUnique()
                    .HasDatabaseName("IX_Option_QuestionId_Key");
                entity.HasIndex(e => e.QuestionId).HasDatabaseName("IX_Option_QuestionId");
            });

            modelBuilder.Entity<JobQuiz>(entity =>
            {
                entity.HasKey(jq => new { jq.JobId, jq.QuizId });
                entity.HasOne(jq => jq.Job)
                    .WithMany(j => j.JobQuizzes)
                    .HasForeignKey(jq => jq.JobId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(jq => jq.Quiz)
                    .WithMany(q => q.JobQuizzes) // This now works with the updated Quiz model
                    .HasForeignKey(jq => jq.QuizId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Permission>()
                .Property(p => p.Id)
                .UseIdentityAlwaysColumn();

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

            var permissions = new[]
            {
                new Permission { Id = 1, Name = "EditQuiz", DisplayName = GenerateDisplayName("EditQuiz") },
                new Permission { Id = 2, Name = "DeleteQuiz", DisplayName = GenerateDisplayName("DeleteQuiz") },
                new Permission { Id = 3, Name = "CreateQuestion", DisplayName = GenerateDisplayName("CreateQuestion") },
                new Permission { Id = 4, Name = "ViewResults", DisplayName = GenerateDisplayName("ViewResults") },
                new Permission { Id = 5, Name = "Admin", DisplayName = GenerateDisplayName("Admin") }
            };
            modelBuilder.Entity<Permission>().HasData(permissions);

            base.OnModelCreating(modelBuilder);
        }

        private string GenerateDisplayName(string name)
        {
            return Regex.Replace(name, "(\\B[A-Z])", " $1");
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