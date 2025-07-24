using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.Backend.Services;
using AssessmentPlatform.Backend.DTOs;
using System.ComponentModel.DataAnnotations;
using AssessmentPlatform.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

                if (createQuizDto == null)
                {
                    _logger.LogWarning("CreateQuizDto is null");
                    return BadRequest("Quiz data is required.");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid: {Errors}",
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

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

        [HttpPost("save")]
        public async Task<ActionResult<QuizResultResponseDto>> QuizSave([FromBody] QuizSubmissionDto submissionDto)
        {
            try
            {
                _logger.LogInformation("Processing quiz submission for Quiz ID: {QuizId}, User ID: {UserId}",
                    submissionDto?.QuizId ?? 0, submissionDto?.UserId ?? "NULL");

                if (submissionDto == null)
                {
                    _logger.LogWarning("QuizSubmissionDto is null");
                    return BadRequest("Submission data is required.");
                }

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid: {Errors}",
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    return BadRequest(ModelState);
                }

                var validationResult = ValidateQuizSubmission(submissionDto);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("Submission validation failed: {Error}", validationResult.ErrorMessage);
                    return BadRequest(validationResult.ErrorMessage);
                }

                var quizResult = await _quizService.SaveQuizResultAsync(submissionDto);

                _logger.LogInformation("Quiz result saved successfully for Quiz ID: {QuizId}, User ID: {UserId}, Score: {Score}",
                    quizResult.QuizId, quizResult.UserId, quizResult.Score);

                return Ok(quizResult);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Argument error saving quiz result: {Message}", ex.Message);
                return BadRequest($"Invalid data: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation saving quiz result: {Message}", ex.Message);
                return BadRequest($"Operation error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error saving quiz result: {Message} | StackTrace: {StackTrace}",
                    ex.Message, ex.StackTrace);
                return StatusCode(500, new
                {
                    error = "An error occurred while saving the quiz result.",
                    details = ex.Message,
                    type = ex.GetType().Name
                });
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

        private (bool IsValid, string ErrorMessage) ValidateQuizSubmission(QuizSubmissionDto submission)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(submission.UserId))
                    return (false, "User ID is required.");

                if (submission.QuizId <= 0)
                    return (false, "Valid Quiz ID is required.");

                if (submission.Answers == null || !submission.Answers.Any())
                    return (false, "At least one answer is required.");

                foreach (var answer in submission.Answers)
                {
                    if (answer.QuestionId <= 0)
                        return (false, "Valid Question ID is required for all answers.");

                    if (answer.SelectedOptions == null || !answer.SelectedOptions.Any())
                        return (false, $"Selected options are required for question {answer.QuestionId}.");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during submission validation");
                return (false, $"Submission validation error: {ex.Message}");
            }
        }
        [HttpGet("results")]
        public async Task<ActionResult<IEnumerable<QuizResultResponseDto>>> GetAllQuizResults()
        {
            try
            {
                _logger.LogInformation("Fetching all quiz results");
                var quizResults = await _quizService.GetAllQuizResultsAsync();
                return Ok(quizResults);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all quiz results");
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving quiz results.",
                    details = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }

        [HttpGet("results/{id}")]
        public async Task<ActionResult<QuizResultResponseDto>> GetQuizResult(int id)
        {
            try
            {
                _logger.LogInformation("Fetching quiz result with ID: {ResultId}", id);
                var quizResult = await _quizService.GetQuizResultByIdAsync(id);

                if (quizResult == null)
                {
                    _logger.LogWarning("Quiz result with ID {ResultId} not found", id);
                    return NotFound($"Quiz result with ID {id} not found.");
                }

                return Ok(quizResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving quiz result with ID: {ResultId}", id);
                return StatusCode(500, new
                {
                    error = "An error occurred while retrieving the quiz result.",
                    details = ex.Message,
                    type = ex.GetType().Name
                });
            }
        }
        [HttpGet("answers/{id}")]
        public async Task<IActionResult> GetQuizAnswers(int id)
        {
            var result = await _quizService.GetQuizAnswerByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = $"No answers found for QuizResultId = {id}" });
            }

            return Ok(result);
        }
    }
}