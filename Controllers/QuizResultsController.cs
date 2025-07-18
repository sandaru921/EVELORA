using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AssessmentPlatform.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QuizResultsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public QuizResultsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/QuizResults/user
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<QuizResult>>> GetQuizResultsForLoggedInUser()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Invalid user ID in token.");

            var results = await _context.QuizResults
                .Where(q => q.UserIdInt == userId)
                .ToListAsync();

            return Ok(results);
        }

        // other methods like Get, Post, Put, Delete can remain as needed
    }
}
