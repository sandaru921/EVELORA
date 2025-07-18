using System;

namespace AssessmentPlatform.Backend.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        public int UserId { get; set; }          // Varchar column
        public int UserIdInt { get; set; }          // FK to Users table
        public int QuizId { get; set; }             // FK to Quizzes table
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public DateTime SubmissionTime { get; set; }
        public int TimeTaken { get; set; }

       
    }
}
