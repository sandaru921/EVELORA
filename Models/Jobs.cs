using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssessmentPlatform.Backend.Models
{
    public class Job
    {
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; } // Required field
        public string? Description { get; set; } // Nullable
        public string? ImageUrl { get; set; }
        [Required]
        public required string JobType { get; set; }
        public string WorkMode { get; set; } = string.Empty; // Default value
        public DateTime ExpiringDate { get; set; } = DateTime.UtcNow; // Default value
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public string? KeyResponsibilities { get; set; }
        [Column(TypeName = "text")]
        public string? EducationalBackground { get; set; }
        [Column(TypeName = "text")]
        public string? TechnicalSkills { get; set; }
        [Column(TypeName = "text")]
        public string? Experience { get; set; }
        [Column(TypeName = "text")]
        public string? SoftSkills { get; set; }
    //public ICollection<JobQuiz> JobQuizzes { get; set; }
}
}