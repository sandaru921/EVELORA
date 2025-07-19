// 1. UPDATED QuizController.cs with better error handling and logging
using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.Backend.Services;
using AssessmentPlatform.Backend.DTOs;
using System.ComponentModel.DataAnnotations;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService _quizService;
        private readonly ILogger<QuizController> _logger;

        public QuizController(IQuizService quizService, ILogger<QuizController> logger)
        {
            _quizService = quizService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult<QuizResponseDto>> CreateQuiz([FromBody] CreateQuizDto createQuizDto)
        {
            try
            {
                _logger.LogInformation("Creating quiz with name: {QuizName}", createQuizDto?.QuizName ?? "NULL");

                // Check if the DTO is null
                if (createQuizDto == null)
                {
                    _logger.LogWarning("CreateQuizDto is null");
                    return BadRequest("Quiz data is required.");
                }

                // Validate the input
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid: {Errors}",
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

                // Additional validation
                var validationResult = ValidateQuizData(createQuizDto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Custom validation failed: {Error}", validationResult.ErrorMessage);
                    return BadRequest(validationResult.ErrorMessage);
                }

                var createdQuiz = await _quizService.CreateQuizAsync(createQuizDto);

                _logger.LogInformation("Quiz created successfully with ID: {QuizId}", createdQuiz.Id);

                return CreatedAtAction(
                    nameof(GetQuiz),
                    new { id = createdQuiz.Id },
                    createdQuiz
                );
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument error creating quiz: {Message}", ex.Message);
                return BadRequest($"Invalid data: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation creating quiz: {Message}", ex.Message);
                return BadRequest($"Operation error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating quiz: {Message} | StackTrace: {StackTrace}",
                    ex.Message, ex.StackTrace);
                return StatusCode(500, new
                {
                    error = "An error occurred while creating the quiz.",
                    details = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            try
            {
                var quiz = await _quizService.GetQuizByIdAsync(id);

                if (quiz == null)
                {
                    return NotFound($"Quiz with ID {id} not found.");
                }

                return Ok(quiz);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz with ID: {QuizId}", id);
                return StatusCode(500, "An error occurred while retrieving the quiz.");
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetAllQuizzes()
        {
            try
            {
                var quizzes = await _quizService.GetAllQuizzesAsync();
                return Ok(quizzes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all quizzes");
                return StatusCode(500, "An error occurred while retrieving quizzes.");
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateQuizData(CreateQuizDto quiz)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(quiz.QuizName))
                    return (false, "Quiz name is required.");

                if (string.IsNullOrWhiteSpace(quiz.JobCategory))
                    return (false, "Job category is required.");

                if (quiz.QuizDuration <= 0)
                    return (false, "Quiz duration must be greater than 0.");

                if (quiz.Questions == null || !quiz.Questions.Any())
                    return (false, "At least one question is required.");

                for (int i = 0; i < quiz.Questions.Count; i++)
                {
                    var question = quiz.Questions[i];

                    if (string.IsNullOrWhiteSpace(question.QuestionText))
                        return (false, $"Question text is required for question {i + 1}.");

                    if (question.Marks <= 0)
                        return (false, $"Question marks must be greater than 0 for question {i + 1}.");

                    if (string.IsNullOrWhiteSpace(question.Type))
                        return (false, $"Question type is required for question {i + 1}.");

                    if (question.Type == "MultipleChoice" || question.Type == "SingleChoice")
                    {
                        if (question.Options == null || !question.Options.Any())
                            return (false, $"Options are required for {question.Type} question {i + 1}.");

                        if (question.CorrectAnswers == null || !question.CorrectAnswers.Any())
                            return (false, $"Correct answers are required for question {i + 1}.");

                        // Validate that correct answers exist in options
                        var optionKeys = question.Options.Select(o => o.Key).ToList();
                        var invalidAnswers = question.CorrectAnswers.Where(ca => !optionKeys.Contains(ca)).ToList();
                        if (invalidAnswers.Any())
                            return (false, $"Invalid correct answers for question {i + 1}: {string.Join(", ", invalidAnswers)}");
                    }
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during validation");
                return (false, $"Validation error: {ex.Message}");
            }
        }
    }
}