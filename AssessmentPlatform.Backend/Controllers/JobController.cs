using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Data; // Ensure correct namespace

namespace AssessmentPlatform.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public JobsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // Get all jobs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Jobs>>> GetJobs()
        {
            return await _context.Jobs.ToListAsync();
        }

        // Get a single job by ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Jobs>> GetJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            return job;
        }

        // Add a new job
        [HttpPost]
        public async Task<ActionResult<Jobs>> AddJob([FromForm] Jobs job, IFormFile image)
        {
            if (image != null)
            {
                string uploadPath = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                string filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                job.ImageUrl = $"/uploads/{fileName}";
            }

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
        }

        // Update a job
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(int id, Jobs updatedJob)
        {
            if (id != updatedJob.Id)
            {
                return BadRequest();
            }

            _context.Entry(updatedJob).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Jobs.Any(j => j.Id == id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // Delete a job
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
