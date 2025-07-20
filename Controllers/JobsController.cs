using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Models;
using AssessmentPlatform.Backend.Data;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace AssessmentPlatform.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<JobsController> _logger;
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=storage1evelora;AccountKey=ToyWFg2+nWEc5C9P9Ekrk+rifpXx4dvpnkm1tmUHSK3g/PO06yuhpQ9A2TSzMHrqcL3dWOAphkBt+AStk0smYQ==;EndpointSuffix=core.windows.net";
        private readonly string _containerName = "jobs";

        public JobsController(AppDbContext context, ILogger<JobsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            _logger.LogInformation("Fetching all jobs");
            try
            {
                var jobs = await _context.Jobs.ToListAsync();
                _logger.LogInformation("Retrieved {Count} jobs", jobs.Count);
                return jobs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching jobs: {Message}", ex.Message);
                return StatusCode(500, $"Failed to retrieve jobs: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(int id)
        {
            _logger.LogInformation("Fetching job with ID: {JobId}", id);
            try
            {
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    _logger.LogWarning("Job with ID: {JobId} not found", id);
                    return NotFound();
                }
                return job;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching job with ID: {JobId}", id);
                return StatusCode(500, $"Failed to retrieve job: {ex.Message}");
            }
        }

        [HttpGet("{id}/Quiz")]
        public async Task<ActionResult<Quiz>> GetJobQuiz(int id)
        {
            _logger.LogInformation("Fetching quiz for job with ID: {JobId}", id);
            try
            {
                var jobQuiz = await _context.JobQuizzes
                    .Include(jq => jq.Quiz)
                    .ThenInclude(q => q.Questions)
                    .ThenInclude(q => q.Options)
                    .FirstOrDefaultAsync(jq => jq.JobId == id);

                if (jobQuiz == null || jobQuiz.Quiz == null)
                {
                    _logger.LogWarning("No quiz found for job with ID: {JobId}", id);
                    return NotFound("No quiz assigned to this job.");
                }

                return jobQuiz.Quiz;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching quiz for job with ID: {JobId}", id);
                return StatusCode(500, $"Failed to retrieve quiz: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<ActionResult<Job>> PostJob([FromForm] JobUploadModel model)
        {
            _logger.LogInformation("Creating new job with title: {Title}", model.Title);
            
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                _logger.LogWarning("Image file is missing");
                return BadRequest("Image file is required.");
            }

            if (string.IsNullOrEmpty(model.WorkMode) || !new[] { "remote", "online", "hybrid" }.Contains(model.WorkMode.ToLower()))
            {
                _logger.LogWarning("Invalid WorkMode: {WorkMode}", model.WorkMode);
                return BadRequest("WorkMode must be 'remote', 'online', or 'hybrid'.");
            }

            if (model.QuizId.HasValue)
            {
                var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == model.QuizId.Value);
                if (!quizExists)
                {
                    _logger.LogWarning("Quiz with ID: {QuizId} does not exist", model.QuizId.Value);
                    return BadRequest($"Quiz with ID {model.QuizId.Value} does not exist.");
                }
            }

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            try
            {
                using (var stream = model.ImageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
                    {
                        ContentType = model.ImageFile.ContentType
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to blob storage: {Message}", ex.Message);
                return StatusCode(500, $"Failed to upload image: {ex.Message}");
            }

            var imageUrl = blobClient.Uri.ToString();

            var job = new Job
            {
                Title = model.Title,
                Description = model.Description,
                ImageUrl = imageUrl,
                JobType = model.JobType,
                ExpiringDate = DateTime.SpecifyKind(DateTime.Parse(model.ExpiringDate), DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = model.CreatedBy,
                WorkMode = model.WorkMode,
                KeyResponsibilities = model.KeyResponsibilities ?? new List<string>(),
                EducationalBackground = model.EducationalBackground ?? new List<string>(),
                TechnicalSkills = model.TechnicalSkills ?? new List<string>(),
                Experience = model.Experience ?? new List<string>(),
                SoftSkills = model.SoftSkills ?? new List<string>(),
                JobQuizzes = model.QuizId.HasValue ? new List<JobQuiz> { new JobQuiz { QuizId = model.QuizId.Value, UserId = null, CreatedAt = DateTime.UtcNow } } : new List<JobQuiz>()
            };

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Adding job to context");
                _context.Jobs.Add(job);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Job saved with ID: {JobId}", job.Id);
                if (model.QuizId.HasValue)
                {
                    _logger.LogInformation("Adding JobQuiz entry for JobId: {JobId}, QuizId: {QuizId}, UserId: null", job.Id, model.QuizId.Value);
                }

                await transaction.CommitAsync();
                _logger.LogInformation("Job and JobQuiz entries committed successfully");
                return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error saving job: {Message}, InnerException: {InnerMessage}", ex.Message, ex.InnerException?.Message);
                return StatusCode(500, $"Failed to upload job due to a database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error saving job: {Message}", ex.Message);
                return StatusCode(500, $"Failed to upload job due to an error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateJob(int id, [FromForm] JobUploadModel model)
        {
            _logger.LogInformation("Updating job with ID: {JobId}", id);
            var job = await _context.Jobs.Include(j => j.JobQuizzes).FirstOrDefaultAsync(j => j.Id == id);
            if (job == null)
            {
                _logger.LogWarning("Job with ID: {JobId} not found", id);
                return NotFound();
            }

            if (string.IsNullOrEmpty(model.WorkMode) || !new[] { "remote", "online", "hybrid" }.Contains(model.WorkMode.ToLower()))
            {
                _logger.LogWarning("Invalid WorkMode: {WorkMode}", model.WorkMode);
                return BadRequest("WorkMode must be 'remote', 'online', or 'hybrid'.");
            }

            if (model.QuizId.HasValue)
            {
                var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == model.QuizId.Value);
                if (!quizExists)
                {
                    _logger.LogWarning("Quiz with ID: {QuizId} does not exist", model.QuizId.Value);
                    return BadRequest($"Quiz with ID {model.QuizId.Value} does not exist.");
                }
            }

            job.Title = model.Title;
            job.Description = model.Description;
            job.JobType = model.JobType;
            job.ExpiringDate = DateTime.SpecifyKind(DateTime.Parse(model.ExpiringDate), DateTimeKind.Utc);
            job.WorkMode = model.WorkMode;
            job.KeyResponsibilities = model.KeyResponsibilities ?? new List<string>();
            job.EducationalBackground = model.EducationalBackground ?? new List<string>();
            job.TechnicalSkills = model.TechnicalSkills ?? new List<string>();
            job.Experience = model.Experience ?? new List<string>();
            job.SoftSkills = model.SoftSkills ?? new List<string>();

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();

                if (!string.IsNullOrEmpty(job.ImageUrl))
                {
                    try
                    {
                        var oldBlobName = Path.GetFileName(new Uri(job.ImageUrl).AbsolutePath);
                        var oldBlobClient = containerClient.GetBlobClient(oldBlobName);
                        await oldBlobClient.DeleteIfExistsAsync();
                        _logger.LogInformation("Deleted old blob: {BlobName}", oldBlobName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error deleting old blob: {Message}", ex.Message);
                    }
                }

                var blobName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var blobClient = containerClient.GetBlobClient(blobName);
                try
                {
                    using (var stream = model.ImageFile.OpenReadStream())
                    {
                        await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
                        {
                            ContentType = model.ImageFile.ContentType
                        });
                    }
                    job.ImageUrl = blobClient.Uri.ToString();
                    _logger.LogInformation("Uploaded new blob: {BlobName}", blobName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading new blob: {Message}", ex.Message);
                    return StatusCode(500, $"Failed to upload image: {ex.Message}");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Removing existing JobQuiz entries for JobId: {JobId}", job.Id);
                _context.JobQuizzes.RemoveRange(job.JobQuizzes);
                job.JobQuizzes.Clear();

                if (model.QuizId.HasValue)
                {
                    job.JobQuizzes.Add(new JobQuiz { QuizId = model.QuizId.Value, UserId = null, CreatedAt = DateTime.UtcNow });
                    _logger.LogInformation("Added new JobQuiz entry for JobId: {JobId}, QuizId: {QuizId}, UserId: null", job.Id, model.QuizId.Value);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                _logger.LogInformation("Job and JobQuiz entries updated successfully for JobId: {JobId}", job.Id);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error updating job: {Message}, InnerException: {InnerMessage}", ex.Message, ex.InnerException?.Message);
                return StatusCode(500, $"Failed to update job due to a database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unexpected error updating job: {Message}", ex.Message);
                return StatusCode(500, $"Failed to update job due to an error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteJob(int id)
        {
            _logger.LogInformation("Deleting job with ID: {JobId}", id);
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                _logger.LogWarning("Job with ID: {JobId} not found", id);
                return NotFound();
            }

            if (!string.IsNullOrEmpty(job.ImageUrl))
            {
                try
                {
                    var blobServiceClient = new BlobServiceClient(_connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                    var blobName = Path.GetFileName(new Uri(job.ImageUrl).AbsolutePath);
                    var blobClient = containerClient.GetBlobClient(blobName);
                    var response = await blobClient.DeleteIfExistsAsync();
                    _logger.LogInformation("Blob deletion result for {BlobName}: {Status}", blobName, response?.GetRawResponse().Status);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deleting blob: {Message}", ex.Message);
                }
            }

            _context.Jobs.Remove(job);

            try
            {
                int affectedRows = await _context.SaveChangesAsync();
                _logger.LogInformation("Database deletion affected {AffectedRows} rows for JobId: {JobId}", affectedRows, id);
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database error deleting job: {Message}, InnerException: {InnerMessage}", ex.Message, ex.InnerException?.Message);
                return StatusCode(500, $"Failed to delete job due to a database error: {ex.InnerException?.Message ?? ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting job: {Message}", ex.Message);
                return StatusCode(500, $"An unexpected error occurred while deleting the job: {ex.Message}");
            }
        }

        public class JobUploadModel
        {
            public string Title { get; set; }
            public string Description { get; set; }
            public IFormFile ImageFile { get; set; }
            public string JobType { get; set; }
            public string ExpiringDate { get; set; }
            public string CreatedBy { get; set; }
            public string WorkMode { get; set; }
            public List<string> KeyResponsibilities { get; set; }
            public List<string> EducationalBackground { get; set; }
            public List<string> TechnicalSkills { get; set; }
            public List<string> Experience { get; set; }
            public List<string> SoftSkills { get; set; }
            public int? QuizId { get; set; }
        }
    }
}