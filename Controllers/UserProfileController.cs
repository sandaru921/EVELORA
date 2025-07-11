using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

// Namespace for the UserProfile controller
namespace AssessmentPlatform.Backend.Controllers
{
    // Define the route and controller attributes
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require JWT authentication for all actions in this controller
    public class UserProfileController : ControllerBase
    {
        // Dependency injection of the database context
        private readonly AppDbContext _context;

        // Constructor to inject the AppDbContext
        public UserProfileController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/userprofile
        // Retrieve the user profile based on the authenticated user's ID
        [HttpGet]
        public async Task<ActionResult<UserProfile>> GetUserProfile()
        {
            // Extract the username (e.g., "Sandaru71") from the JWT token claims
            var userIdStr = User.Identity.Name;
            if (string.IsNullOrEmpty(userIdStr))
            {
                // Return unauthorized if no user ID is found in the token
                return Unauthorized("User not authenticated.");
            }

            // Query the Users table to find the user by username or email
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userIdStr || u.Email == userIdStr);
            if (user == null)
            {
                // Return not found if the user doesn't exist
                return NotFound("User not found.");
            }

            // Query the UserProfiles table using the shadow property UserIdInt
            var userProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => EF.Property<int>(u, "UserIdInt") == user.Id);

            if (userProfile == null)
            {
                // Create a new user profile if none exists
                userProfile = new UserProfile
                {
                    UserId = userIdStr, // Set the frontend-compatible UserId
                    ProfilePicture = "https://via.placeholder.com/150.png?text=User", // Default profile picture
                    Education = "", // Initialize empty fields
                    WorkExperience = "",
                    Skills = "",
                    Name = "", // Name will be updated by the user later
                    Age = null,
                    Gender = "",
                    LinkedIn = ""
                };
                // Set the shadow property UserIdInt for the foreign key relationship
                _context.Entry(userProfile).Property("UserIdInt").CurrentValue = user.Id;
                _context.UserProfiles.Add(userProfile); // Add the new profile to the context
                await _context.SaveChangesAsync(); // Save changes to the database
            }

            return Ok(userProfile); // Return the user profile
        }

        // POST: api/userprofile/update
        // Update the user profile with new data
        [HttpPost("update")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfile userProfile)
        {
            // Extract the username from the JWT token claims
            var userIdStr = User.Identity.Name;
            if (string.IsNullOrEmpty(userIdStr))
            {
                // Return unauthorized if no user ID is found
                return Unauthorized("User not authenticated.");
            }

            // Query the Users table to find the user
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == userIdStr || u.Email == userIdStr);
            if (user == null)
            {
                // Return not found if the user doesn't exist
                return NotFound("User not found.");
            }

            if (userProfile == null)
            {
                // Return bad request if the profile data is invalid
                return BadRequest("Invalid user profile data.");
            }

            // Query the existing user profile
            var existingProfile = await _context.UserProfiles
                .FirstOrDefaultAsync(u => EF.Property<int>(u, "UserIdInt") == user.Id);

            if (existingProfile == null)
            {
                // Create a new profile if none exists
                userProfile.UserId = userIdStr; // Set frontend-compatible UserId
                _context.Entry(userProfile).Property("UserIdInt").CurrentValue = user.Id; // Set shadow property
                _context.UserProfiles.Add(userProfile); // Add the new profile
            }
            else
            {
                // Update existing profile with new data
                existingProfile.ProfilePicture = userProfile.ProfilePicture;
                existingProfile.Education = userProfile.Education;
                existingProfile.WorkExperience = userProfile.WorkExperience;
                existingProfile.Skills = userProfile.Skills;
                existingProfile.Name = userProfile.Name;
                existingProfile.Age = userProfile.Age;
                existingProfile.Gender = userProfile.Gender;
                existingProfile.LinkedIn = userProfile.LinkedIn;
                existingProfile.UserId = userIdStr; // Update frontend-compatible UserId
                _context.Entry(existingProfile).Property("UserIdInt").CurrentValue = user.Id; // Ensure shadow property is correct
            }

            await _context.SaveChangesAsync(); // Save changes to the database
            return Ok(new { message = "Profile updated successfully" }); // Return success response
        }
    }
}