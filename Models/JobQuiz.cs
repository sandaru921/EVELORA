using System;
using System.ComponentModel.DataAnnotations;

namespace AssessmentPlatform.Backend.Models
{
    public class JobQuiz
    {
        [Key]
        public int Id { get; set; }
        public int JobId { get; set; }
        public Job Job { get; set; }
        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
        public int? UserId { get; set; }
        public User User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}