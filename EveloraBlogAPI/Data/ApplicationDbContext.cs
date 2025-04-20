// // Data/ApplicationDbContext.cs
// using Microsoft.EntityFrameworkCore;
// using EverolaBlogAPI.Models;

// namespace EverolaBlogAPI.Data
// {
//     public class ApplicationDbContext : DbContext
//     {
//         public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//             : base(options)
//         {
//         }

//         public DbSet<Blog> Blogs { get; set; }

//         protected override void OnModelCreating(ModelBuilder modelBuilder)
//         {
//             base.OnModelCreating(modelBuilder);

//             // Seed some initial blog data
//             modelBuilder.Entity<Blog>().HasData(
//                 new Blog
//                 {
//                     Id = 1,
//                     Title = "How To Tackle Security Testing And Challenges",
//                     Content = "Security testing is a crucial aspect of software development...",
//                     Category = "Software",
//                     ImageUrl = "/images/blogs/security.jpeg",
//                     CreatedAt = DateTime.SpecifyKind(new DateTime(2023, 4, 7), DateTimeKind.Utc)
//                 },
//                 new Blog
//                 {
//                     Id = 2,
//                     Title = "Agile Testing: It's a new age of testing",
//                     Content = "Agile methodology has revolutionized the way we approach testing...",
//                     Category = "Software",
//                     ImageUrl = "/images/blogs/agile.png",
//                     CreatedAt = DateTime.SpecifyKind(new DateTime(2022, 11, 23), DateTimeKind.Utc)
//                 }
//             );
//         }
//     }
// }

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
                .Property(b => b.Slug)
                .HasMaxLength(200);
        }
    }
}
