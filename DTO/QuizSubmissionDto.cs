namespace AssessmentPlatform.Backend.DTOs
{
    public class QuizSubmissionDto
    {
        public string UserId { get; set; }
        public int QuizId { get; set; }
        public int TimeTaken { get; set; } // In seconds
        public List<AnswerSubmissionDto> Answers { get; set; }
    }

    public class AnswerSubmissionDto
    {
        public int QuestionId { get; set; }
        public List<string> SelectedOptions { get; set; }
    }

    public class QuizResultResponseDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; } // Added for username
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public DateTime SubmissionTime { get; set; }
        public int TimeTaken { get; set; }
        public List<AnswerDetailsDto> Answers { get; set; } // Added for answer details
    }

    public class AnswerDetailsDto
    {
        public int QuestionId { get; set; }
        public List<string> SelectedOptions { get; set; }
        public bool IsCorrect { get; set; }
        public int MarksObtained { get; set; }
    }
}