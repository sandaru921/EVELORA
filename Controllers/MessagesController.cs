using System.Text.Json;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace AssessmentPlatform.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(AppDbContext context, ILogger<MessagesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(string userId)
        {
            try
            {
                _logger.LogInformation($"Attempting to get messages for user {userId}");
                
                var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("nameid")?.Value;
                
                if (string.IsNullOrEmpty(tokenUserId))
                {
                    _logger.LogWarning("Unauthorized - No user ID found in token");
                    return Unauthorized("Authentication failed");
                }

                if (userId != tokenUserId)
                {
                    _logger.LogWarning($"Forbidden - User {tokenUserId} tried to access messages for {userId}");
                    return Forbid();
                }

                var messages = await _context.Messages
                    .Where(m => m.Sender == userId || m.Recipient == userId)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                _logger.LogInformation($"Successfully retrieved {messages.Count} messages for user {userId}");
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving messages for user {userId}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Message>>> GetAllMessages()
        {
            try
            {
                _logger.LogInformation("Admin requesting all messages");
                
                var messages = await _context.Messages
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                _logger.LogInformation($"Returning {messages.Count} messages to admin");
                return Ok(messages);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all messages");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Message>> SendMessage([FromBody] Message message)
        {
            try
            {
                _logger.LogInformation($"Attempting to send message: {JsonSerializer.Serialize(message)}");
                
                var tokenUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("nameid")?.Value;
                var tokenUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
                var isAdmin = tokenUserRole == "Admin";

                // Validate required fields
                if (message == null)
                {
                    _logger.LogWarning("Message object is null");
                    return BadRequest(new { message = "Message data is required" });
                }

                if (string.IsNullOrWhiteSpace(message.Text))
                {
                    _logger.LogWarning("Message text is empty");
                    return BadRequest(new { message = "Message text is required" });
                }

                if (string.IsNullOrWhiteSpace(message.Recipient))
                {
                    _logger.LogWarning("Recipient is empty");
                    return BadRequest(new { message = "Recipient is required" });
                }

                // Validate sender
                if (!isAdmin && (string.IsNullOrWhiteSpace(message.Sender) || message.Sender != tokenUserId))
                {
                    _logger.LogWarning($"Invalid sender: {message.Sender} (token user: {tokenUserId})");
                    return Forbid();
                }

                // Set final values
                message.Sender = isAdmin ? "Admin" : tokenUserId;
                message.Role = isAdmin ? "Admin" : "User";
                message.Timestamp = DateTimeOffset.UtcNow;

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Message sent successfully from {message.Sender} to {message.Recipient}");
                return CreatedAtAction(nameof(GetMessages), new { userId = message.Sender }, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending message");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}