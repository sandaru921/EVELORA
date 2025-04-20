using System;
using System.ComponentModel.DataAnnotations;

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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; }
    }
}