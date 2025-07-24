namespace AssessmentPlatform.Backend.DTO
{

    public class QuizResultAnswerResponseDto
    {
        public int QuizResultId { get; set; }
        public List<AnswerDto> Answers { get; set; }
    }

    public class AnswerDto
    {
        public int QuestionId { get; set; }
        public List<string> SelectedOptions { get; set; }
        public bool IsCorrect { get; set; }
        public int MarksObtained { get; set; }
    }

}