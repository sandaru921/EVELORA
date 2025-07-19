using AssessmentPlatform.Backend.Data;

namespace AssessmentPlatform.Backend.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public int QuizResultId { get; set; }
        public int QuestionId { get; set; }
        public ICollection<string> SelectedOptions { get; set; } // Optional, may be removed
        public bool IsCorrect { get; set; }
        public int MarksObtained { get; set; }

        public QuizResult QuizResult { get; set; }
        public Question Question { get; set; }
        public ICollection<AnswerSelectedOption> AnswerSelectedOptions { get; set; } // Added
    }
}