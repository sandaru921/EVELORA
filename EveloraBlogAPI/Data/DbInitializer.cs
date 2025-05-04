




using EveloraBlogAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace EveloraBlogAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context, ILogger? logger = null)
        {
            if (logger != null)
            {
                logger.LogInformation("Some message");
            }

            try
            {
                // Check if the Blogs table exists
                bool tableExists = false;
                try
                {
                    // Try to query the table to see if it exists
                    tableExists = context.Blogs.Any();
                }
                catch
                {
                    // Table might not exist yet
                    logger?.LogWarning("Blogs table does not exist yet.");
                }

                // If the table exists and has data, don't seed
                if (tableExists && context.Blogs.Any())
                {
                    logger?.LogInformation("Database already has blog data - skipping seeding.");
                    return;
                }

                logger?.LogInformation("Seeding database with initial blog data...");

                var blogs = new Blog[]
                {
                    new Blog
                    {
                        Title = "How To Tackle Security Testing And Challenges",
                        Content = @"<h2>Overview</h2>
<p>Cyber security ventures stated that cyber crime is expected to grow by 15% every year. They predicted that those crimes might cost $10.5 trillion annually on a global scale by the year 2025. Security testing plays a vital role in preventing most of these crimes.</p>
<p>In this article, let's look at five security testing challenges and how to tackle them.</p>
<ul>
  <li><strong>Challenge 1:</strong> Focusing on security requirements in the requirements phase</li>
  <li><strong>Challenge 2:</strong> Writing test cases for security testing and updating them regularly</li>
  <li><strong>Challenge 3:</strong> Including API security</li>
  <li><strong>Challenge 4:</strong> Security testing skill set</li>
  <li><strong>Challenge 5:</strong> Selection of tools</li>
</ul>",
                        Category = "Software",
                        ImageUrl = "/images/blogs/security-testing.jpg",
                        CreatedAt = DateTime.SpecifyKind(new DateTime(2023, 4, 7), DateTimeKind.Utc)
                    },
                    new Blog
                    {
                        Title = "Agile Testing: It's a new age of testing",
                        Content = @"<h2>Overview</h2>
<p>Agile methodology has revolutionized the way we approach testing in software development. This article discusses how agile testing differs from traditional methods and why it's becoming the preferred approach for modern development teams.</p>",
                        Category = "Software",
                        ImageUrl = "/images/blogs/agile-testing.jpg",
                        CreatedAt = DateTime.SpecifyKind(new DateTime(2022, 11, 23), DateTimeKind.Utc)
                    }
                };

                // Generate slugs for each blog
                foreach (var blog in blogs)
                {
                    blog.GenerateSlug();
                }

                context.Blogs.AddRange(blogs);
                context.SaveChanges();
                logger?.LogInformation($"Added {blogs.Length} blog posts to the database.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred while seeding the database.");
                throw; // Re-throw to be caught by the calling method
            }
        }
    }
}