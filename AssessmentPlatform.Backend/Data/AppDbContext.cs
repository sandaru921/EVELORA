using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Jobs> Jobs { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // protected override void OnModelCreating(ModelBuilder modelBuilder)
        // {
        //     base.OnModelCreating(modelBuilder);

        //     // Ignore Login class to prevent EF Core from treating it as an entity
        //     // modelBuilder.Ignore<Login>();

        //     // Apply unique constraints on Username and Email
        //     modelBuilder.Entity<User>()
        //         .HasIndex(u => u.Username)
        //         .IsUnique();

        //     modelBuilder.Entity<User>()
        //         .HasIndex(u => u.Email)
        //         .IsUnique();
        // }
    }
}
