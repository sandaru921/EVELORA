using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Data;

namespace AssessmentPlatform.Controllers
{
    [Route("api/messages")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(string userId)
        {
            var messages = await _context.Messages
                .Where(m => m.Sender == userId || m.Recipient == userId)
                .ToListAsync();
            return Ok(messages);
        }

        [HttpPost]
        public async Task<ActionResult<Message>> SendMessage([FromBody] Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(message);
        }
    }
}