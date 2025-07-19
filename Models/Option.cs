namespace AssessmentPlatform.Backend.Models
{
    public class Option
    {
        public int Id { get; set; }
        public string Key { get; set; } // A, B, C, D
        public string Value { get; set; }

        // Foreign key to Question
        public int QuestionId { get; set; }
        public Question Question { get; set; }
    }
}
