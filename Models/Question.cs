using System.Text.Json.Serialization;

namespace AssessmentPlatform.Backend.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string CodeSnippet { get; set; }
        public string ImageURL { get; set; }
        public string Type { get; set; } // e.g., MultipleChoice, SingleChoice, ImageBased
        public int Marks { get; set; }

        public ICollection<Option> Options { get; set; }
        public ICollection<string> CorrectAnswers { get; set; }

        // Foreign key to Quiz
        public int QuizId { get; set; }

        public Quiz? Quiz { get; set; }
    }
}
