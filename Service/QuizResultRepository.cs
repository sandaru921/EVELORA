using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssessmentPlatform.Backend.Services
{
    public class QuizResultRepository : IQuizResultRepository
    {
        private readonly AppDbContext _context;

        public QuizResultRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuizResults>> GetQuizResultsAsync()
        {
            return await _context.QuizResults.ToListAsync();
        }
        
        public async Task<List<QuizResults>> GetQuizResultsByUserIdAsync(string userId)
        {
            return await _context.QuizResults.Where(r => r.UserId == userId).ToListAsync();
        }

        public async Task AddQuizResultAsync(QuizResults result)
        {
            _context.QuizResults.Add(result);
            await _context.SaveChangesAsync();
        }

        public async Task<List<QuizResults>> GetQuizResultsWithDetailsAsync()
        {
            return await _context.QuizResults
                .Include(r => r.Quiz)
                .Include(r => r.User)
                .ToListAsync();
        }
    }
}

