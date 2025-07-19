// AssessmentPlatform.Backend/Models/JobQuiz.cs
namespace AssessmentPlatform.Backend.Models
{
    public class JobQuiz
    {
        public int JobId { get; set; }
        public Job Job { get; set; }

        public int QuizId { get; set; }
        public Quiz Quiz { get; set; }
    }
}