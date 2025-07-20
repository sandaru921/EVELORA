using AssessmentPlatform.Backend.Models;
using AssessmentPlatform.Backend.Services;
using AssessmentPlatform.Backend.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using AssessmentPlatform.Backend.Hubs;
using Microsoft.AspNetCore.Authorization;

namespace AssessmentPlatform.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly QuizAnalysisService _analysisService;
        private readonly IHubContext<AnalysisHub> _hubContext;
        private readonly IQuizResultRepository _repository;

        public AnalysisController(QuizAnalysisService analysisService, IHubContext<AnalysisHub> hubContext, IQuizResultRepository repository)
        {
            _analysisService = analysisService;
            _hubContext = hubContext;
            _repository = repository;
        }

        [HttpGet("rankings")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRankings()
        {
            var rankings = await _analysisService.GetRankingsAsync();
            
            // Convert to DTOs to ensure proper serialization
            var rankingDtos = rankings.Select(r => new RankingDTO
            {
                Rank = r.Rank,
                Name = r.Name,
                Marks = r.Marks,
                TotalMarks = r.TotalMarks,
                Percentage = r.Percentage,
                Category = r.Category,
                TimeTaken = r.TimeTaken,
                JobRole = r.JobRole,
                QuizName = r.QuizName
            }).ToList();
            
            // Debug logging to verify TotalMarks is set
            if (rankingDtos.Any())
            {
                var firstRanking = rankingDtos.First();
                Console.WriteLine($"Debug - First ranking TotalMarks: {firstRanking.TotalMarks}");
                Console.WriteLine($"Debug - First ranking JSON: {System.Text.Json.JsonSerializer.Serialize(firstRanking)}");
            }
            
            return Ok(rankingDtos);
        }

        [HttpGet("rankings/category/{category}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRankingsByCategory(string category)
        {
            var rankings = await _analysisService.GetRankingsByCategoryAsync(category);
            return Ok(rankings);
        }

        [HttpGet("rankings/jobrole/{jobRole}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFairRankingsByJobRole(string jobRole)
        {
            var rankings = await _analysisService.GetFairRankingsByJobRoleAsync(jobRole);
            return Ok(rankings);
        }

        [HttpGet("categories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAvailableCategories()
        {
            var categories = await _analysisService.GetAvailableCategoriesAsync();
            return Ok(categories);
        }

        [HttpGet("statistics")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRankingStatistics()
        {
            var statistics = await _analysisService.GetRankingStatisticsAsync();
            return Ok(statistics);
        }

        [HttpGet("statistics/jobrole/{jobRole}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobRoleStatistics(string jobRole)
        {
            var statistics = await _analysisService.GetJobRoleStatisticsAsync(jobRole);
            return Ok(statistics);
        }

        [HttpPost("submitResult")]
        [AllowAnonymous]
        public async Task<IActionResult> SubmitResult([FromBody] QuizResultDTO resultDto)
        {
            if (!ModelState.IsValid)
        {
                return BadRequest(ModelState);
            }

            var result = new QuizResults
            {
                UserId = resultDto.UserId,
                QuizId = resultDto.QuizId,
                Score = resultDto.Score,
                TotalMarks = resultDto.TotalMarks,
                TimeTaken = resultDto.TimeTaken,
                UserIdInt = resultDto.UserIdInt,
                SubmissionTime = DateTime.UtcNow
            };

            await _repository.AddQuizResultAsync(result);

            // Create response DTO
            var responseDto = new QuizResultResponseDTO
            {
                Id = result.Id,
                UserId = result.UserId,
                QuizId = result.QuizId,
                Score = result.Score,
                TotalMarks = result.TotalMarks,
                SubmissionTime = result.SubmissionTime,
                TimeTaken = result.TimeTaken,
                UserIdInt = result.UserIdInt,
                Percentage = result.TotalMarks > 0 ? (double)result.Score / result.TotalMarks * 100 : 0
            };

            // Send real-time update via SignalR
            await _hubContext.Clients.All.SendAsync("ReceiveQuizResult", responseDto);
            
            return Ok(responseDto);
        }

        [HttpGet("user/{userId}/results")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserResults(string userId)
        {
            var results = await _repository.GetQuizResultsByUserIdAsync(userId);
            var responseDtos = results.Select(r => new QuizResultResponseDTO
            {
                Id = r.Id,
                UserId = r.UserId,
                QuizId = r.QuizId,
                Score = r.Score,
                TotalMarks = r.TotalMarks,
                SubmissionTime = r.SubmissionTime,
                TimeTaken = r.TimeTaken,
                UserIdInt = r.UserIdInt,
                Percentage = r.TotalMarks > 0 ? (double)r.Score / r.TotalMarks * 100 : 0
            }).ToList();

            return Ok(responseDtos);
        }
    }
}