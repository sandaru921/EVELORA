using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Data
{
    // Database context class for interacting with PostgreSQL using Entity Framework Core
    public class AppDbContext : DbContext
    {
        // DbSet for the Users table, representing platform users 
        public DbSet<User> Users { get; set; }

        // DbSet for the Jobs table, storing job listings created by recruiters
        public DbSet<Job> Jobs { get; set; }

        // DbSet for the Messages table, storing communication between users
        public DbSet<Message> Messages { get; set; }

        // Constructor to initialize the database context with configuration options
        // Parameters: options - Options for configuring the database context (e.g., connection string)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Configures the database model, defining schema and constraints for PostgreSQL
        // Parameters: modelBuilder - Builder for defining entity configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configures the Message entity's Id to use PostgreSQL's IDENTITY ALWAYS column
            modelBuilder.Entity<Message>()
                .Property(m => m.Id)
                .UseIdentityAlwaysColumn();

            // Configures the User entity's Id to use PostgreSQL's IDENTITY ALWAYS column
            modelBuilder.Entity<User>()
                .Property(u => u.Id)
                .UseIdentityAlwaysColumn();

            // Configures the Job entity's Id to use PostgreSQL's IDENTITY ALWAYS column
            modelBuilder.Entity<Job>()
                .Property(j => j.Id)
                .UseIdentityAlwaysColumn();
        }
    }

    // Entity class representing a message sent between users in the platform
    public class Message
    {
        // Primary key for the message, auto-incremented by PostgreSQL
        public int Id { get; set; }

        // Content of the message
        public string Text { get; set; }

        // Identifier of the user sending the message
        public string Sender { get; set; }

        // Identifier of the user receiving the message
        public string Recipient { get; set; }

        // Timestamp when the message was sent, stored as a string
        public string Timestamp { get; set; }
    }
}