using AssessmentPlatform.Backend.Data;
using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.DTO;
using Microsoft.AspNetCore.Authorization;
using AssessmentPlatform.Backend.Service; // Ensure this namespace contains UserService
using AssessmentPlatform.Backend.DTO;
using AssessmentPlatform.Backend.Models;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;

namespace AssessmentPlatform.Backend.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly GoogleAuthService _googleAuthService;
        private readonly AppDbContext _context;

        public UserController(UserService userService, GoogleAuthService googleAuthService, AppDbContext context)
        {
            _userService = userService;
            _googleAuthService = googleAuthService;
            _context = context;
        }

        //POST: Register a new user
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

        //POST: Log in and get token with permissions
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
        
        [HttpPost("google-signup")]
        public async Task<IActionResult> GoogleSignUp([FromBody] GoogleRegisterDto dto)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(dto.Credential);
                var email = payload.Email;

                var existingUser = await _userService.GetByEmailAsync(email);
                if (existingUser != null)
                {
                    return Ok(new { success = true, message = "User already registered." });
                }

                var user = new User
                {
                    Username = dto.Username,
                    Email = email,
                    HashPassword = _userService.HashPassword(Guid.NewGuid().ToString()), // Dummy
                    IsGoogleUser = true
                };

                await _userService.CreateUserAsync(user);

                return Ok(new { success = true, message = "User registered via Google." });
            }
            catch (InvalidJwtException)
            {
                return BadRequest(new { success = false, message = "Invalid Google token." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Server error: " + ex.Message });
            }
        }
        
        
        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            var payload = await _googleAuthService.VerifyGoogleTokenAsync(dto.Credential);
            if (payload == null)
                return Unauthorized(new { message = "Invalid Google token" });

            // Check if user exists
            var user = await _userService.GetByEmailAsync(payload.Email);
            if (user == null)
                return Unauthorized(new { message = "User does not exist. Please register first." });

            // Optional: Sync Google ID or update user data

            // Generate JWT
            var token = _userService.GenerateJwtToken(user);
            var permissions = user.UserPermissions?.Select(p => p.Permission.Name).ToList() ?? new List<string>();
            
            return Ok(new
            {
                token,
                permissions
            });
        }
        
        //GET: A sample protected endpoint (token required)
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
        
        //GET: All users with their permissions (Admin only)
        // GET: api/user/with-permissions
        [HttpGet("with-permissions")]
        [Authorize(Roles = "Admin")] // Optional: Restrict to Admins
        public async Task<IActionResult> GetAllUsersWithPermissions()
        {
            var users = await _userService.GetAllUsersWithPermissionsAsync();
            return Ok(users);
        }

    }
}
