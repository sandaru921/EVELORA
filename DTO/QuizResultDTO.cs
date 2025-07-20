namespace AssessmentPlatform.Backend.DTO
{
    public class QuizResultDTO
    {
        public string UserId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public int TimeTaken { get; set; }
        public int UserIdInt { get; set; }
    }

    public class QuizResultResponseDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalMarks { get; set; }
        public DateTime SubmissionTime { get; set; }
        public int TimeTaken { get; set; }
        public int UserIdInt { get; set; }
        public double Percentage { get; set; }
    }
} 