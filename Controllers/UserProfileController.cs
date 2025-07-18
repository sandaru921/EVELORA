using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace AssessmentPlatform.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserProfileController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/userprofile
        [HttpGet]
        public async Task<ActionResult<UserProfile>> GetUserProfile()
        {
            var userIdStr = User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("User not authenticated.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userIdStr || u.Email == userIdStr);

            if (user == null)
                return NotFound("User not found.");

            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => EF.Property<int>(u, "UserIdInt") == user.Id);

            if (userProfile == null)
            {
                userProfile = new UserProfile
                {
                    UserId = userIdStr,
                    ProfilePicture = "https://via.placeholder.com/150.png?text=User",
                    Education = "",
                    WorkExperience = "",
                    Skills = "",
                    Name = "",
                    Age = null,
                    Gender = "",
                    LinkedIn = "",
                    Title = "" // newly added field
                };

                _context.Entry(userProfile).Property("UserIdInt").CurrentValue = user.Id;
                _context.UserProfiles.Add(userProfile);
                await _context.SaveChangesAsync();
            }

            return Ok(userProfile);
        }

        // POST: api/userprofile/update
        [HttpPost("update")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfile userProfile)
        {
            var userIdStr = User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("User not authenticated.");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userIdStr || u.Email == userIdStr);

            if (user == null)
                return NotFound("User not found.");

            if (userProfile == null)
                return BadRequest("Invalid user profile data.");

            var existingProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => EF.Property<int>(u, "UserIdInt") == user.Id);

            if (existingProfile == null)
            {
                userProfile.UserId = userIdStr;
                _context.Entry(userProfile).Property("UserIdInt").CurrentValue = user.Id;
                _context.UserProfiles.Add(userProfile);
            }
            else
            {
                existingProfile.ProfilePicture = userProfile.ProfilePicture;
                existingProfile.Education = userProfile.Education;
                existingProfile.WorkExperience = userProfile.WorkExperience;
                existingProfile.Skills = userProfile.Skills;
                existingProfile.Name = userProfile.Name;
                existingProfile.Age = userProfile.Age;
                existingProfile.Gender = userProfile.Gender;
                existingProfile.LinkedIn = userProfile.LinkedIn;
                existingProfile.Title = userProfile.Title;
                existingProfile.UserId = userIdStr;
                _context.Entry(existingProfile).Property("UserIdInt").CurrentValue = user.Id;
            }

            await _context.SaveChangesAsync();
            return Ok(existingProfile);
        }
    }
}
