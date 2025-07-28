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
        public DbSet<OtpVerification> OtpVerifications { get; set; } = null!;
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }

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

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.QuizName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.JobCategory)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.QuizLevel)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.QuizDuration)
                    .IsRequired();

               

                // Create unique index on QuizName
                entity.HasIndex(e => e.QuizName)
                    .IsUnique()
                    .HasDatabaseName("IX_Quiz_QuizName");
            });

            // Enhanced Question configuration
            modelBuilder.Entity<Question>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.QuestionText)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(e => e.Type)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.CodeSnippet)
                    .HasMaxLength(4000)
                    .IsRequired(false);

                entity.Property(e => e.ImageURL)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(e => e.Marks)
                    .IsRequired();

                // Configure CorrectAnswers as JSON column with proper conversion
                entity.Property(e => e.CorrectAnswers)
                    .HasConversion(
                        v => v != null && v.Any() ? string.Join(";", v) : string.Empty,
                        v => !string.IsNullOrEmpty(v) ? v.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>()
                    )
                    .HasColumnType("text")
                    .IsRequired();

                // Foreign key relationship
                entity.HasOne(e => e.Quiz)
                    .WithMany(e => e.Questions)
                    .HasForeignKey(e => e.QuizId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Create index on QuizId for better query performance
                entity.HasIndex(e => e.QuizId)
                    .HasDatabaseName("IX_Question_QuizId");
            });

            // Enhanced Option configuration
            modelBuilder.Entity<Option>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .UseIdentityAlwaysColumn();

                entity.Property(e => e.Key)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.Value)
                    .IsRequired()
                    .HasMaxLength(500);

                // Foreign key relationship
                entity.HasOne(e => e.Question)
                    .WithMany(e => e.Options)
                    .HasForeignKey(e => e.QuestionId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                // Create composite unique index to prevent duplicate option keys per question
                entity.HasIndex(e => new { e.QuestionId, e.Key })
                    .IsUnique()
                    .HasDatabaseName("IX_Option_QuestionId_Key");

                // Create index on QuestionId for better query performance
                entity.HasIndex(e => e.QuestionId)
                    .HasDatabaseName("IX_Option_QuestionId");
            });

            base.OnModelCreating(modelBuilder);

            
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
        }
        // Helper: convert camelCase to spaced words
        private string GenerateDisplayName(string name)
        {
            return Regex.Replace(name, "(\\B[A-Z])", " $1");
        }
    }

    // Keep your existing Message class
    public class Message
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Sender { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;
    }
}
