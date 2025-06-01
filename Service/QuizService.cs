using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using AssessmentPlatform.Backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AssessmentPlatform.Backend.Services
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<QuizService> _logger;

        public QuizService(AppDbContext context, ILogger<QuizService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<QuizResponseDto> CreateQuizAsync(CreateQuizDto createQuizDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Starting quiz creation for: {QuizName}", createQuizDto.QuizName);

                var quiz = new Quiz
                {
                    QuizName = createQuizDto.QuizName?.Trim(),
                    JobCategory = createQuizDto.JobCategory?.Trim(),
                    Description = createQuizDto.Description?.Trim(),
                    QuizDuration = createQuizDto.QuizDuration,
                    QuizLevel = createQuizDto.QuizLevel?.Trim(),
                    Questions = new List<Question>()
                };

                _logger.LogInformation("Created quiz object, adding {QuestionCount} questions", createQuizDto.Questions.Count);

                // Add questions
                foreach (var questionDto in createQuizDto.Questions)
                {
                    var question = new Question
                    {
                        QuestionText = questionDto.QuestionText?.Trim(),
                        CodeSnippet = questionDto.CodeSnippet?.Trim(),
                        ImageURL = questionDto.ImageURL?.Trim(),
                        Type = questionDto.Type?.Trim(),
                        Marks = questionDto.Marks,
                        CorrectAnswers = questionDto.CorrectAnswers ?? new List<string>(),
                        Options = new List<Option>()
                    };

                    // Add options
                    if (questionDto.Options != null)
                    {
                        foreach (var optionDto in questionDto.Options)
                        {
                            var option = new Option
                            {
                                Key = optionDto.Key?.Trim(),
                                Value = optionDto.Value?.Trim()
                            };
                            question.Options.Add(option);
                        }
                    }

                    quiz.Questions.Add(question);
                }

                _context.Quizzes.Add(quiz);

                _logger.LogInformation("Saving quiz to database");
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("Quiz saved successfully with ID: {QuizId}", quiz.Id);

                return new QuizResponseDto
                {
                    Id = quiz.Id,
                    QuizName = quiz.QuizName,
                    JobCategory = quiz.JobCategory,
                    Description = quiz.Description,
                    QuizDuration = quiz.QuizDuration,
                    QuizLevel = quiz.QuizLevel,
                    QuestionCount = quiz.Questions.Count,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating quiz: {Message}", ex.Message);
                throw;
            }
        }

        public async Task<Quiz?> GetQuizByIdAsync(int id)
        {
            try
            {
                return await _context.Quizzes
                    .Include(q => q.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(q => q.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz by ID: {QuizId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<Quiz>> GetAllQuizzesAsync()
        {
            try
            {
                return await _context.Quizzes
                    .Include(q => q.Questions)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all quizzes");
                throw;
            }
        }
    }
}