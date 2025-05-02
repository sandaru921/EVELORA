using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.DTO;
using Microsoft.AspNetCore.Authorization;
using AssessmentPlatform.Backend.Service; // Ensure this namespace contains UserService

namespace AssessmentPlatform.Backend.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        // POST: api/user/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Username) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Username, email, and password are required.");
            }

            var (user, errorMessage) = await _userService.RegisterUserAsync(dto);

            if (!string.IsNullOrEmpty(errorMessage))
            {
                return Conflict(new { message = errorMessage });
            }

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email
            });
        }

        // POST: api/user/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO login)
        {
            if (string.IsNullOrEmpty(login.Email) && string.IsNullOrEmpty(login.Username))
                return BadRequest("Email or Username is required.");

            if (string.IsNullOrEmpty(login.Password))
                return BadRequest("Password is required.");

            var (user, token) = await _userService.AuthenticateUserAsync(login);

            if (user == null || string.IsNullOrEmpty(token))
                return Unauthorized("Invalid username/email or password.");

            return Ok(new
            {
                message = "Login successful",
                token = token
                // userId = user.Id,
                // username = user.Username,
                // email = user.Email
            });
        }


        // GET: api/user/protected
        [HttpGet("protected")]
        [Authorize]
        public IActionResult Protected()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            return Ok(new
            {
                message = "You have accessed a protected endpoint!",
                claims = claims
            });
        }

    }
}
