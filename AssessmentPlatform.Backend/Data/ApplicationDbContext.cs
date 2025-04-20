using Microsoft.EntityFrameworkCore;
using EverolaBlogAPI.Models;

namespace EverolaBlogAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure the Blog entity
            modelBuilder.Entity<Blog>()
                .HasKey(b => b.Id);

            modelBuilder.Entity<Blog>()
                .Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200);

            modelBuilder.Entity<Blog>()
                .Property(b => b.Content)
                .IsRequired();

            modelBuilder.Entity<Blog>()
                .Property(b => b.Category)
                .IsRequired()
                .HasMaxLength(50);

            // Seed some initial blog data
            modelBuilder.Entity<Blog>().HasData(
                new Blog
                {
                    Id = 1,
                    Title = "How To Tackle Security Testing And Challenges",
                    Content = "Security testing is a crucial aspect of software development that helps identify vulnerabilities and weaknesses in applications. This comprehensive guide explores the various challenges faced during security testing and provides practical strategies to overcome them effectively.",
                    Category = "Software",
                    ImageUrl = "/images/blogs/security.jpeg",
                    CreatedAt = new DateTime(2023, 4, 7)
                },
                new Blog
                {
                    Id = 2,
                    Title = "Agile Testing: It's a new age of testing",
                    Content = "Agile methodology has revolutionized the way we approach testing in software development. This article discusses how agile testing differs from traditional methods and why it's becoming the preferred approach for modern development teams.",
                    Category = "Software",
                    ImageUrl = "/images/blogs/agile.png",
                    CreatedAt = new DateTime(2022, 11, 23)
                }
            );
        }
    }
}