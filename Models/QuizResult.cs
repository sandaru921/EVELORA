using AssessmentPlatform.Backend.DTO;

namespace AssessmentPlatform.Backend.Models
{
    public class QuizResult
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public DateTime SubmissionTime { get; set; }
        public int TimeTaken { get; set; } // In seconds

        public Quiz Quiz { get; set; }
       public ICollection<AnswerDto> Answers { get; set; }
        public User User { get; internal set; }
    }
}