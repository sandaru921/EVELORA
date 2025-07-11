using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Jobs> Jobs { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

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

            modelBuilder.Entity<Jobs>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Message>()
                .Property(m => m.Id)
                .UseIdentityAlwaysColumn();
            modelBuilder.Entity<Message>()
                .Property(m => m.Timestamp)
                .HasColumnType("timestamp with time zone")
                .IsRequired()
                .HasDefaultValueSql("NOW()");
            modelBuilder.Entity<Message>()
                .Property(m => m.Role)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}