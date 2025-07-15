using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


// namespace EveloraBlogAPI.Models
namespace AssessmentPlatform.Backend.Models
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Slug { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Generate slug from title
        public void GenerateSlug()
        {
            if (string.IsNullOrEmpty(Title))
                return;
                
            // Convert to lowercase
            Slug = Title.ToLower();
            
            // Replace special characters
            Slug = Slug.Replace(" ", "-")
                .Replace("&", "and")
                .Replace("?", "")
                .Replace("!", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace(":", "")
                .Replace(";", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("[", "")
                .Replace("]", "")
                .Replace("\"", "")
                .Replace("'", "")
                .Replace("/", "-")
                .Replace("\\", "-");
                
            // Remove any non-alphanumeric characters except hyphens
            Slug = Regex.Replace(Slug, @"[^a-z0-9\-]", "");
            
            // Remove any double hyphens
            Slug = Regex.Replace(Slug, @"-+", "-");
            
            // Trim hyphens from start and end
            Slug = Slug.Trim('-');
            
            // Ensure we have a slug
            if (string.IsNullOrEmpty(Slug))
            {
                Slug = "blog-" + Guid.NewGuid().ToString().Substring(0, 8);
            }
        }
    }
}
