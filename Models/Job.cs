using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AssessmentPlatform.Backend.Models
{
    public class Job
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        [Required]
        public string JobType { get; set; }
        public string WorkMode { get; set; } = string.Empty;
        public DateTime ExpiringDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; }
        [Column(TypeName = "text")]
        public List<string> KeyResponsibilities { get; set; } = new List<string>();
        [Column(TypeName = "text")]
        public List<string> EducationalBackground { get; set; } = new List<string>();
        [Column(TypeName = "text")]
        public List<string> TechnicalSkills { get; set; } = new List<string>();
        [Column(TypeName = "text")]
        public List<string> Experience { get; set; } = new List<string>();
        [Column(TypeName = "text")]
        public List<string> SoftSkills { get; set; } = new List<string>();
        public ICollection<JobQuiz> JobQuizzes { get; set; } = new List<JobQuiz>();
        
      

    }
}