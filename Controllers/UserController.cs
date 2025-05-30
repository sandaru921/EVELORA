using AssessmentPlatform.Backend.Data;
using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.DTO;
using Microsoft.AspNetCore.Authorization;
using AssessmentPlatform.Backend.Service; // Ensure this namespace contains UserService
using AssessmentPlatform.Backend.DTO;

namespace AssessmentPlatform.Backend.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService, AppDbContext context)
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
                message = "User registered successfully",
            });
        }

        // POST: api/user/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO login)
        {
            var (user, token, permissions) = await _userService.AuthenticateUserAsync(login);
            
            if (string.IsNullOrEmpty(login.Email) && string.IsNullOrEmpty(login.Username))
                return BadRequest("Email or Username is required.");

            if (string.IsNullOrEmpty(login.Password))
                return BadRequest("Password is required.");

            if (user == null || string.IsNullOrEmpty(token))
                return Unauthorized("Invalid credentials");
            
            return Ok(new
            {
                token = token,
                permissions = permissions
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
        
        // POST: api/user/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var error = await _userService.ResetPasswordAsync(dto);
            if (error != null)
                return BadRequest(error);

            return Ok("Password has been reset successfully.");
        }
    }
}
