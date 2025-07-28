namespace AssessmentPlatform.Backend.DTO
{
    public class QuizUpdateDto
    {
        public int Id { get; set; }
        public string QuizName { get; set; }
        public string JobCategory { get; set; }
        public string Description { get; set; }
        public string QuizLevel { get; set; }
        public int QuizDuration { get; set; }
        public List<QuestionUpdateDto> Questions { get; set; }
    }

    public class QuestionUpdateDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string Type { get; set; }
        public string CodeSnippet { get; set; }
        public string ImageURL { get; set; }
        public int Marks { get; set; }
        public List<string> CorrectAnswers { get; set; }
        public List<OptionUpdateDto> Options { get; set; }
    }

    public class OptionUpdateDto
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

}