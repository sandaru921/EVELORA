// // Models/Blog.cs
// using System;
// using System.ComponentModel.DataAnnotations;

// namespace EverolaBlogAPI.Models
// {
//     public class Blog
//     {
//         [Key]
//         public int Id { get; set; }
        
//         [Required]
//         [MaxLength(200)]
//         public string Title { get; set; }
        
//         [Required]
//         public string Content { get; set; }
        
//         [Required]
//         [MaxLength(50)]
//         public string Category { get; set; }
        
//         public string ImageUrl { get; set; }
        
//         public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
//         public DateTime? UpdatedAt { get; set; }
//     }
// }

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EverolaBlogAPI.Models
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } 
        
        [Required]
        public string Content { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Category { get; set; } 
        
        public string ImageUrl { get; set; } 
        
        public string Slug { get; set; } 
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
        
        // Generate slug from title
        public void GenerateSlug()
        {
            if (string.IsNullOrEmpty(Title))
                return;
                
            Slug = Title.ToLower()
                .Replace(" ", "-")
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
                .Replace("'", "");
                
            // Remove any double hyphens
            Slug = Regex.Replace(Slug, @"-+", "-");
            
            // Trim hyphens from start and end
            Slug = Slug.Trim('-');
        }
    }
}
