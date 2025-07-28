namespace AssessmentPlatform.Backend.DTOs
{
    public class CreateQuizDto
    {
        public string QuizName { get; set; }
        public string JobCategory { get; set; }
        public string Description { get; set; }
        public int QuizDuration { get; set; } // In minutes
        public string QuizLevel { get; set; }
        public List<CreateQuestionDto> Questions { get; set; } = new List<CreateQuestionDto>();
    }

    public class CreateQuestionDto
    {
        public string QuestionText { get; set; }
        public string? CodeSnippet { get; set; }
        public string? ImageURL { get; set; }
        public string Type { get; set; } // e.g., MultipleChoice, SingleChoice, ImageBased
        public int Marks { get; set; }
        public List<CreateOptionDto> Options { get; set; } = new List<CreateOptionDto>();
        public List<string> CorrectAnswers { get; set; } = new List<string>();
    }

    public class CreateOptionDto
    {
        public string Key { get; set; } // A, B, C, D
        public string Value { get; set; }
    }

    public class QuizResponseDto
    {
        public int Id { get; set; }
        public string QuizName { get; set; }
        public string JobCategory { get; set; }
        public string Description { get; set; }
        public int QuizDuration { get; set; }
        public string QuizLevel { get; set; }
        public int QuestionCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}