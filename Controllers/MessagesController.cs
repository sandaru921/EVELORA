using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssessmentPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require JWT authentication
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/messages/{userId}
        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            try
            {
                var messages = await _context.Messages
                    .Where(m => m.Sender == userId || m.Recipient == userId)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving messages: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving messages.");
            }
        }

        // GET: api/messages (for admins only)
        [HttpGet]
        //[Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Message>>> GetAllMessages()
        {
            try
            {
                var messages = await _context.Messages
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
                return Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving all messages: {ex.Message}");
                return StatusCode(500, "An error occurred while retrieving all messages.");
            }
        }

        // POST: api/messages
        [HttpPost]
        public async Task<ActionResult<Message>> SendMessage(Message message)
        {
            if (message == null || string.IsNullOrEmpty(message.Text) || string.IsNullOrEmpty(message.Sender) || string.IsNullOrEmpty(message.Recipient) || string.IsNullOrEmpty(message.Role))
            {
                return BadRequest("Message, Sender, Recipient, and Role are required.");
            }

            // Determine role based on JWT claims (simplified; use real role from token in production)
            var userRole = User.IsInRole("Admin") ? "Admin" : "User";
            message.Role = userRole;
            message.Timestamp = DateTimeOffset.UtcNow;

            try
            {
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetMessages), new { userId = message.Sender }, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving message: {ex.Message}");
                return StatusCode(500, "An error occurred while saving the message.");
            }
        }
        
        
    }
}