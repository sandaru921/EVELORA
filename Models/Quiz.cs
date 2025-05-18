namespace AssessmentPlatform.Backend.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string QuizName { get; set; }
        public int QuizDuration { get; set; }
        public DateOnly QuizDate { get; set; }
        public int JobId { get; set; }
        public Jobs Jobs { get; set; }

    }
}
