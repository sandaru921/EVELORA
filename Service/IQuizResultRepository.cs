using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssessmentPlatform.Backend.Services
{
    public interface IQuizResultRepository
    {
        Task<List<QuizResults>> GetQuizResultsAsync();
        Task<List<QuizResults>> GetQuizResultsByUserIdAsync(string userId);
        Task AddQuizResultAsync(QuizResults result);
        Task<List<QuizResults>> GetQuizResultsWithDetailsAsync();
    }
}