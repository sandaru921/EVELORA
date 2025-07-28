using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.Backend.Services;
using AssessmentPlatform.Backend.DTO;
using AssessmentPlatform.Backend.DTOs;
using AssessmentPlatform.Backend.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            if (createQuizDto == null)
                return BadRequest("Quiz data is required.");

            var validationResult = ValidateQuizData(createQuizDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var createdQuiz = await _quizService.CreateQuizAsync(createQuizDto);
            return CreatedAtAction(nameof(GetQuiz), new { id = createdQuiz.Id }, createdQuiz);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Quiz>> GetQuiz(int id)
        {
            var quiz = await _quizService.GetQuizByIdAsync(id);
            if (quiz == null)
                return NotFound();
            return Ok(quiz);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Quiz>>> GetAllQuizzes()
        {
            var quizzes = await _quizService.GetAllQuizzesAsync();
            return Ok(quizzes);
        }

        [HttpPost("save")]
        public async Task<ActionResult<QuizResultResponseDto>> QuizSave([FromBody] QuizSubmissionDto submissionDto)
        {
            if (submissionDto == null)
                return BadRequest("Submission data is required.");

            var validationResult = ValidateQuizSubmission(submissionDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var quizResult = await _quizService.SaveQuizResultAsync(submissionDto);
            return Ok(quizResult);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] QuizUpdateDto updateQuizDto)
        {
            if (updateQuizDto == null)
                return BadRequest("Quiz data is required.");

            var validationResult = ValidateQuizData(updateQuizDto);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.ErrorMessage);

            var updatedQuiz = await _quizService.UpdateQuizAsync(id, updateQuizDto);
            if (updatedQuiz == null)
                return NotFound();

            return Ok(updatedQuiz);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            bool deleted = await _quizService.DeleteQuizAsync(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [HttpGet("results")]
        public async Task<ActionResult<IEnumerable<QuizResultResponseDto>>> GetAllQuizResults()
        {
            var results = await _quizService.GetAllQuizResultsAsync();
            return Ok(results);
        }

        [HttpGet("results/{id}")]
        public async Task<ActionResult<QuizResultResponseDto>> GetQuizResult(int id)
        {
            var result = await _quizService.GetQuizResultByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpGet("answers/{id}")]
        public async Task<ActionResult<QuizResultAnswerResponseDto>> GetQuizAnswers(int id)
        {
            var result = await _quizService.GetQuizAnswerByIdAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        private (bool IsValid, string ErrorMessage) ValidateQuizData(CreateQuizDto quiz)
        {
            if (string.IsNullOrWhiteSpace(quiz.QuizName))
                return (false, "Quiz name is required.");
            if (quiz.Questions == null || !quiz.Questions.Any())
                return (false, "At least one question is required.");
            return (true, string.Empty);
        }

        private (bool IsValid, string ErrorMessage) ValidateQuizData(QuizUpdateDto quiz)
        {
            if (string.IsNullOrWhiteSpace(quiz.QuizName))
                return (false, "Quiz name is required.");
            if (quiz.Questions == null || !quiz.Questions.Any())
                return (false, "At least one question is required.");
            return (true, string.Empty);
        }

        private (bool IsValid, string ErrorMessage) ValidateQuizSubmission(QuizSubmissionDto submission)
        {
            if (string.IsNullOrWhiteSpace(submission.UserId))
                return (false, "User ID is required.");
            if (submission.QuizId <= 0)
                return (false, "Quiz ID is required.");
            if (submission.Answers == null || !submission.Answers.Any())
                return (false, "At least one answer is required.");
            return (true, string.Empty);
        }
    }
}
