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
        public ICollection<Answer> Answers { get; set; }
        public User User { get; internal set; }
    };
public class AnswerSelectedOption
{
    public int AnswerId { get; set; }
    public string SelectedOption { get; set; }

    public Answer Answer { get; set; }

};
    public class Answer
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public ICollection<string> SelectedOptions { get; set; }
        public bool IsCorrect { get; set; }
        public int MarksObtained { get; set; }

        // Foreign key to QuizResult
        public int QuizResultId { get; set; }
        public QuizResult QuizResult { get; set; }
        
          public Question Question { get; set; }
    public ICollection<AnswerSelectedOption> AnswerSelectedOptions { get; set; }
    }
};
