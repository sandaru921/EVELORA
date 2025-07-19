// AssessmentPlatform.Backend/Controllers/JobsController.cs
using Microsoft.EntityFrameworkCore;
using AssessmentPlatform.Backend.Models;
using AssessmentPlatform.Backend.Data; // Add this line if AppDbContext is in the Data namespace
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AssessmentPlatform.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=storage1evelora;AccountKey=ToyWFg2+nWEc5C9P9Ekrk+rifpXx4dvpnkm1tmUHSK3g/PO06yuhpQ9A2TSzMHrqcL3dWOAphkBt+AStk0smYQ==;EndpointSuffix=core.windows.net";
        private readonly string _containerName = "jobs";

        public JobsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            return await _context.Jobs.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }
            return job;
        }

        [HttpPost]
        public async Task<ActionResult<Job>> PostJob([FromForm] JobUploadModel model)
        {
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            if (string.IsNullOrEmpty(model.WorkMode) || !new[] { "remote", "online", "hybrid" }.Contains(model.WorkMode.ToLower()))
            {
                return BadRequest("WorkMode must be 'remote', 'online', or 'hybrid'.");
            }

            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            using (var stream = model.ImageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
                {
                    ContentType = model.ImageFile.ContentType
                });
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
                KeyResponsibilities = model.KeyResponsibilities,
                EducationalBackground = model.EducationalBackground,
                TechnicalSkills = model.TechnicalSkills,
                Experience = model.Experience,
                SoftSkills = model.SoftSkills,
                JobQuizzes = model.QuizIds.Select(qid => new JobQuiz { QuizId = qid }).ToList()
            };

            _context.Jobs.Add(job);

            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetJobs), new { id = job.Id }, job);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving job: {ex.Message}");
                return StatusCode(500, "Failed to upload job due to a database error.");
            }
        }

        [HttpPut("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> UpdateJob(int id, [FromForm] JobUploadModel model)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(model.WorkMode) || !new[] { "remote", "online", "hybrid" }.Contains(model.WorkMode.ToLower()))
            {
                return BadRequest("WorkMode must be 'remote', 'online', or 'hybrid'.");
            }

            job.Title = model.Title;
            job.Description = model.Description;
            job.JobType = model.JobType;
            job.ExpiringDate = DateTime.SpecifyKind(DateTime.Parse(model.ExpiringDate), DateTimeKind.Utc);
            job.WorkMode = model.WorkMode;
            job.KeyResponsibilities = model.KeyResponsibilities;
            job.EducationalBackground = model.EducationalBackground;
            job.TechnicalSkills = model.TechnicalSkills;
            job.Experience = model.Experience;
            job.SoftSkills = model.SoftSkills;

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
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting old blob: {ex.Message}");
                    }
                }

                var blobName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var blobClient = containerClient.GetBlobClient(blobName);
                using (var stream = model.ImageFile.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
                    {
                        ContentType = model.ImageFile.ContentType
                    });
                }
                job.ImageUrl = blobClient.Uri.ToString();
            }

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating job: {ex.Message}");
                return StatusCode(500, "Failed to update job due to a database error.");
            }
        }

        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            Console.WriteLine($"Attempting to delete job with ID: {id}, Title: {job.Title}");

            if (!string.IsNullOrEmpty(job.ImageUrl))
            {
                try
                {
                    var blobServiceClient = new BlobServiceClient(_connectionString);
                    var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                    var blobName = Path.GetFileName(new Uri(job.ImageUrl).AbsolutePath);
                    var blobClient = containerClient.GetBlobClient(blobName);
                    var response = await blobClient.DeleteIfExistsAsync();
                    Console.WriteLine($"Blob deletion result for {blobName}: {response?.GetRawResponse().Status}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting blob: {ex.Message}");
                }
            }

            _context.Jobs.Remove(job);

            try
            {
                int affectedRows = await _context.SaveChangesAsync();
                Console.WriteLine($"Database deletion affected {affectedRows} rows");
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database error deleting job: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, "Failed to delete job due to a database error.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error deleting job: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while deleting the job.");
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
            public string? KeyResponsibilities { get; set; }
            public string? EducationalBackground { get; set; }
            public string? TechnicalSkills { get; set; }
            public string? Experience { get; set; }
            public string? SoftSkills { get; set; }
            public List<int> QuizIds { get; set; } = new List<int>(); // New property for quiz assignments
        }
    }
}