using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Models;

namespace AssessmentPlatform.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}