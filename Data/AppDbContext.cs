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
        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<QuizResult> QuizResults { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<AnswerSelectedOption> AnswerSelectedOptions { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure existing entities
            modelBuilder.Entity<Message>()
                .Property(m => m.Id)
                .UseIdentityAlwaysColumn();

            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .UseIdentityAlwaysColumn();

            modelBuilder.Entity<Jobs>()
                .Property(j => j.Id)
                .UseIdentityAlwaysColumn();

            // Enhanced Quiz configuration
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

            // Enhanced Question configuration
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
                        v => !string.IsNullOrEmpty(v) ? v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>()
                    )
                    .HasColumnType("text")
                    .IsRequired();
                entity.HasOne(e => e.Quiz)
                    .WithMany(e => e.Questions)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                entity.HasIndex(e => e.QuizId).HasDatabaseName("IX_Question_QuizId");
            });

            // Enhanced Option configuration
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
                entity.HasIndex(e => new { e.QuestionId, e.Key }).IsUnique().HasDatabaseName("IX_Option_QuestionId_Key");
                entity.HasIndex(e => e.QuestionId).HasDatabaseName("IX_Option_QuestionId");
            });

            // QuizResult configuration
            modelBuilder.Entity<QuizResult>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.UserId).IsRequired().HasMaxLength(450); // String for DTO
                entity.Property(e => e.QuizId).IsRequired();
                entity.Property(e => e.Score).IsRequired();
                entity.Property(e => e.TotalMarks).IsRequired();
                entity.Property(e => e.SubmissionTime).IsRequired();
                entity.Property(e => e.TimeTaken).IsRequired();
                entity.Property<int>("UserIdInt"); // Shadow property for foreign key
                entity.HasOne(e => e.Quiz)
                    .WithMany()
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey("UserIdInt") // Use shadow property
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();
                entity.HasIndex(e => e.QuizId).HasDatabaseName("IX_QuizResult_QuizId");
                entity.HasIndex(e => e.UserId).HasDatabaseName("IX_QuizResult_UserId");
            });

            // Answer configuration
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityAlwaysColumn();
                entity.Property(e => e.QuizResultId).IsRequired();
                entity.Property(e => e.QuestionId).IsRequired();
                entity.Property(e => e.IsCorrect).IsRequired();
                entity.Property(e => e.MarksObtained).IsRequired();
                entity.HasOne(e => e.QuizResult)
                    .WithMany(e => e.Answers)
                    .HasForeignKey(e => e.QuizResultId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                entity.HasOne(e => e.Question)
                    .WithMany()
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();
                entity.HasIndex(e => e.QuizResultId).HasDatabaseName("IX_Answer_QuizResultId");
                entity.HasIndex(e => e.QuestionId).HasDatabaseName("IX_Answer_QuestionId");
            });

            // AnswerSelectedOption configuration
            modelBuilder.Entity<AnswerSelectedOption>(entity =>
            {
                entity.HasKey(e => new { e.AnswerId, e.SelectedOption });
                entity.Property(e => e.SelectedOption).IsRequired().HasMaxLength(10);
                entity.HasOne(e => e.Answer)
                    .WithMany(a => a.AnswerSelectedOptions)
                    .HasForeignKey(e => e.AnswerId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                entity.HasIndex(e => e.AnswerId).HasDatabaseName("IX_AnswerSelectedOption_AnswerId");
            });

            // Configure UserPermission and Permission
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

            // Seed permissions
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
        public string Text { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }

    public class AnswerSelectedOption
    {
        public int AnswerId { get; set; }
        public string SelectedOption { get; set; }
        public Answer Answer { get; set; }
    }
}