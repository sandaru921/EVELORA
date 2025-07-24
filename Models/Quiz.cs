namespace AssessmentPlatform.Backend.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string QuizName { get; set; }
        public string JobCategory { get; set; }
        public string Description { get; set; }
        public int QuizDuration { get; set; } // In minutes
        public string QuizLevel { get; set; }

        public ICollection<Question> Questions { get; set; }
    }
}