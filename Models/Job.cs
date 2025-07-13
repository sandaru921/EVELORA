using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssessmentPlatform.Backend.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        public string JobType { get; set; }

        public string WorkMode { get; set; }

        public DateTime ExpiringDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedBy { get; set; }

        // New fields to store paragraphs or point-form details
        [Column(TypeName = "text")]
        public string? KeyResponsibilities { get; set; }

        [Column(TypeName = "text")]
        public string? EducationalBackground { get; set; }

        [Column(TypeName = "text")]
        public string? TechnicalSkills { get; set; }

        [Column(TypeName = "text")]
        public string? Experience { get; set; }

        [Column(TypeName = "text")]
        public string? SoftSkills { get; set; }
    }
}