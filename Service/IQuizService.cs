using AssessmentPlatform.Backend.DTOs;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Services
{
    public interface IQuizService
    {
        Task<QuizResponseDto> CreateQuizAsync(CreateQuizDto createQuizDto);
        Task<Quiz?> GetQuizByIdAsync(int id);
        Task<IEnumerable<Quiz>> GetAllQuizzesAsync();
        
    }
}