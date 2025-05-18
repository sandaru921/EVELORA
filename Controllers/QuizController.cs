using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/Quiz/QuestionType
        [HttpPost("QuestionType")]
        public IActionResult AddQuestionType([FromBody] QuestionType dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TypeName))
                return BadRequest("Type name is required.");

            var newType = new QuestionType { TypeName = dto.TypeName };
            _context.QuestionTypes.Add(newType);
            _context.SaveChanges();

            return Ok(new { message = "Question type added", id = newType.TypeId });
        }

        // GET: api/Quiz/QuestionTypes
        [HttpGet("QuestionTypes")]
        public IActionResult GetQuestionTypes()
        {
            var types = _context.QuestionTypes.ToList();
            return Ok(types);
        }
    }
}
