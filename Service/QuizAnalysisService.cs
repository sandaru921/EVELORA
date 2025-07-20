using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssessmentPlatform.Backend.Services
{
    public class QuizAnalysisService
    {
        private readonly IQuizResultRepository _repository;
        private readonly AppDbContext _context;

        public QuizAnalysisService(IQuizResultRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<List<Ranking>> GetRankingsAsync()
        {
            var results = await _repository.GetQuizResultsAsync();

            // FUTURE: Replace this with dynamic user lookup
            // var userNames = await _context.Users
            //     .ToDictionaryAsync(u => u.Id.ToString(), u => u.Username);
            
            // CURRENT: Hardcoded for testing
            var userNames = new Dictionary<string, string>
            {
                { "8", "Test User" }
            };

            // Get quiz categories and details
            var quizDetails = await _context.Quizzes
                .ToDictionaryAsync(q => q.Id, q => new { q.JobCategory, q.QuizName });

            // Group results by job category for fair comparison
            var groupedResults = results
                .Where(r => quizDetails.ContainsKey(r.QuizId))
                .GroupBy(r => quizDetails[r.QuizId].JobCategory)
                .ToList();

            var allRankings = new List<Ranking>();

            foreach (var group in groupedResults)
            {
                var category = group.Key;
                var categoryResults = group.ToList();

                // Calculate rankings within this job category only
                var categoryRankings = categoryResults
                    .Select(r => new Ranking
                    {
                        Name = userNames.ContainsKey(r.UserId) ? userNames[r.UserId] : r.UserId,
                        Marks = r.Score,
                        TotalMarks = r.TotalMarks,
                        Percentage = r.TotalMarks > 0 ? (double)r.Score / r.TotalMarks * 100 : 0,
                        TimeTaken = r.TimeTaken,
                        Category = category,
                        JobRole = category, // Add job role for clarity
                        QuizName = quizDetails[r.QuizId].QuizName
                    })
                    .OrderByDescending(r => r.Marks)
                    .ThenBy(r => r.TimeTaken)
                    .ToList();

                // Assign ranks within this category
                for (int i = 0; i < categoryRankings.Count; i++)
                {
                    categoryRankings[i].Rank = i + 1;
                    
                    // Handle ties within category
                    if (i > 0 && categoryRankings[i].Marks == categoryRankings[i - 1].Marks && 
                        categoryRankings[i].TimeTaken == categoryRankings[i - 1].TimeTaken)
                    {
                        categoryRankings[i].Rank = categoryRankings[i - 1].Rank;
                    }
                }

                allRankings.AddRange(categoryRankings);
            }

            // Sort by category, then by rank within category
            return allRankings
                .OrderBy(r => r.Category)
                .ThenBy(r => r.Rank)
                .ToList();
        }

        public async Task<List<Ranking>> GetRankingsByCategoryAsync(string category)
        {
            var results = await _repository.GetQuizResultsAsync();
            
            // Get quizzes in the specified category
            var quizIdsInCategory = await _context.Quizzes
                .Where(q => q.JobCategory == category)
                .Select(q => q.Id)
                .ToListAsync();

            // Filter results by category
            var categoryResults = results.Where(r => quizIdsInCategory.Contains(r.QuizId)).ToList();

            // Get user names - for now hardcode user "8" as "Test User"
            var userNames = new Dictionary<string, string>
            {
                { "8", "Test User" }
            };

            // Get quiz details for this category
            var quizDetails = await _context.Quizzes
                .Where(q => q.JobCategory == category)
                .ToDictionaryAsync(q => q.Id, q => new { q.JobCategory, q.QuizName });

            var rankings = categoryResults
                .Select(r => new Ranking
                {
                    Name = userNames.ContainsKey(r.UserId) ? userNames[r.UserId] : r.UserId,
                    Marks = r.Score,
                    TotalMarks = r.TotalMarks,
                    Percentage = r.TotalMarks > 0 ? (double)r.Score / r.TotalMarks * 100 : 0,
                    TimeTaken = r.TimeTaken,
                    Category = category,
                    JobRole = category,
                    QuizName = quizDetails.ContainsKey(r.QuizId) ? quizDetails[r.QuizId].QuizName : "Unknown Quiz"
                })
                .OrderByDescending(r => r.Marks)
                .ThenBy(r => r.TimeTaken)
                .ToList();

            // Assign ranks
            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].Rank = i + 1;
                
                // Handle ties
                if (i > 0 && rankings[i].Marks == rankings[i - 1].Marks && 
                    rankings[i].TimeTaken == rankings[i - 1].TimeTaken)
                {
                    rankings[i].Rank = rankings[i - 1].Rank;
                }
            }

            return rankings;
        }

        public async Task<List<string>> GetAvailableCategoriesAsync()
        {
            return await _context.Quizzes
                .Select(q => q.JobCategory)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> GetRankingStatisticsAsync()
        {
            var results = await _repository.GetQuizResultsAsync();
            
            if (!results.Any())
            {
                return new Dictionary<string, object>
                {
                    { "totalParticipants", 0 },
                    { "averageScore", 0.0 },
                    { "averageTime", 0.0 },
                    { "topScore", 0 },
                    { "categories", new List<string>() },
                    { "categoryStats", new Dictionary<string, object>() }
                };
            }

            // Get quiz details
            var quizDetails = await _context.Quizzes
                .ToDictionaryAsync(q => q.Id, q => new { q.JobCategory, q.QuizName });

            // Group by category for fair statistics
            var groupedResults = results
                .Where(r => quizDetails.ContainsKey(r.QuizId))
                .GroupBy(r => quizDetails[r.QuizId].JobCategory)
                .ToList();

            var categoryStats = new Dictionary<string, object>();

            foreach (var group in groupedResults)
            {
                var category = group.Key;
                var categoryResults = group.ToList();

                var categoryData = new Dictionary<string, object>
                {
                    { "participants", categoryResults.Select(r => r.UserId).Distinct().Count() },
                    { "averageScore", Math.Round(categoryResults.Average(r => (double)r.Score / r.TotalMarks * 100), 2) },
                    { "averageTime", Math.Round(categoryResults.Average(r => r.TimeTaken), 2) },
                    { "topScore", Math.Round(categoryResults.Max(r => (double)r.Score / r.TotalMarks * 100), 2) },
                    { "totalMarks", categoryResults.First().TotalMarks }, // Same for all in category
                    { "quizName", quizDetails[categoryResults.First().QuizId].QuizName }
                };

                categoryStats[category] = categoryData;
            }

            var totalParticipants = results.Select(r => r.UserId).Distinct().Count();
            var averageScore = results.Average(r => (double)r.Score / r.TotalMarks * 100);
            var averageTime = results.Average(r => r.TimeTaken);
            var topScore = results.Max(r => (double)r.Score / r.TotalMarks * 100);
            var categories = await GetAvailableCategoriesAsync();

            return new Dictionary<string, object>
            {
                { "totalParticipants", totalParticipants },
                { "averageScore", Math.Round(averageScore, 2) },
                { "averageTime", Math.Round(averageTime, 2) },
                { "topScore", Math.Round(topScore, 2) },
                { "categories", categories },
                { "categoryStats", categoryStats }
            };
        }

        // New method for getting rankings by job role with fair comparison
        public async Task<List<Ranking>> GetFairRankingsByJobRoleAsync(string jobRole)
        {
            return await GetRankingsByCategoryAsync(jobRole);
        }

        // New method for getting overall statistics by job role
        public async Task<Dictionary<string, object>> GetJobRoleStatisticsAsync(string jobRole)
        {
            var categoryStats = await GetRankingStatisticsAsync();
            var categoryStatsDict = categoryStats["categoryStats"] as Dictionary<string, object>;
            
            if (categoryStatsDict != null && categoryStatsDict.ContainsKey(jobRole))
            {
                return categoryStatsDict[jobRole] as Dictionary<string, object>;
            }

            return new Dictionary<string, object>
            {
                { "participants", 0 },
                { "averageScore", 0.0 },
                { "averageTime", 0.0 },
                { "topScore", 0.0 },
                { "totalMarks", 0 },
                { "quizName", "No Quiz Available" }
            };
        }
    }
}