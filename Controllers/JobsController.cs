using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace AssessmentPlatform.Backend.Controllers
{
    // Defines the API route for job-related operations 
    [Route("api/[controller]")]
    [ApiController]
    public class JobsController : ControllerBase
    {
        // Database context for interacting with PostgreSQL
        private readonly AppDbContext _context;

        // Azure Blob Storage connection string for storing job images
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=storage1evelora;AccountKey=ToyWFg2+nWEc5C9P9Ekrk+rifpXx4dvpnkm1tmUHSK3g/PO06yuhpQ9A2TSzMHrqcL3dWOAphkBt+AStk0smYQ==;EndpointSuffix=core.windows.net";

        // Azure Blob Storage container name for job images
        private readonly string _containerName = "jobs";

        // Constructor to inject the database context
        public JobsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Jobs
        // Retrieves all jobs from the database
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Job>>> GetJobs()
        {
            // Fetches all jobs from the Jobs table asynchronously
            return await _context.Jobs.ToListAsync();
        }

        // GET: api/Jobs/{id}
        // Retrieves a specific job by its ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(int id)
        {
            // Finds a job by ID in the Jobs table
            var job = await _context.Jobs.FindAsync(id);

            // Returns 404 if the job is not found
            if (job == null)
            {
                return NotFound();
            }

            // Returns the job details
            return job;
        }

        // POST: api/Jobs
        // Creates a new job with an image uploaded to Azure Blob Storage
        [HttpPost]
        public async Task<ActionResult<Job>> PostJob([FromForm] JobUploadModel model)
        {
            // Validates that an image file is provided
            if (model.ImageFile == null || model.ImageFile.Length == 0)
            {
                return BadRequest("Image file is required.");
            }

            // Validates WorkMode (must be 'remote', 'online', or 'hybrid')
            if (string.IsNullOrEmpty(model.WorkMode) || !new[] { "remote", "online", "hybrid" }.Contains(model.WorkMode.ToLower()))
            {
                return BadRequest("WorkMode must be 'remote', 'online', or 'hybrid'.");
            }

            // Initializes Azure Blob Storage client
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

            // Creates the blob container if it doesn't exist
            await containerClient.CreateIfNotExistsAsync();

            // Generates a unique blob name using GUID and file extension
            var blobName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
            var blobClient = containerClient.GetBlobClient(blobName);

            // Uploads the image file to Azure Blob Storage
            using (var stream = model.ImageFile.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobHttpHeaders
                {
                    ContentType = model.ImageFile.ContentType
                });
            }

            // Gets the URL of the uploaded image
            var imageUrl = blobClient.Uri.ToString();

            // Creates a new Job entity with provided data
            var job = new Job
            {
                Title = model.Title,
                Description = model.Description,
                ImageUrl = imageUrl,
                JobType = model.JobType,
                // Ensures ExpiringDate is in UTC
                ExpiringDate = DateTime.SpecifyKind(DateTime.Parse(model.ExpiringDate), DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = model.CreatedBy,
                WorkMode = model.WorkMode,
                // Assigns additional job details
                KeyResponsibilities = model.KeyResponsibilities,
                EducationalBackground = model.EducationalBackground,
                TechnicalSkills = model.TechnicalSkills,
                Experience = model.Experience,
                SoftSkills = model.SoftSkills
            };

            // Adds the job to the database
            _context.Jobs.Add(job);

            try
            {
                // Saves changes to PostgreSQL
                await _context.SaveChangesAsync();

                // Returns 201 Created with the job details
                return CreatedAtAction(nameof(GetJobs), new { id = job.Id }, job);
            }
            catch (Exception ex)
            {
                // Logs and returns 500 error if database save fails
                Console.WriteLine($"Error saving job: {ex.Message}");
                return StatusCode(500, "Failed to upload job due to a database error.");
            }
        }

        // PUT: api/Jobs/{id}
        // Updates an existing job, optionally replacing its image
        [HttpPut("{id}")]
        [AllowAnonymous] // Note: Consider adding authorization for production
        public async Task<IActionResult> UpdateJob(int id, [FromForm] JobUploadModel model)
        {
            // Finds the job by ID
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            // Validates WorkMode
            if (string.IsNullOrEmpty(model.WorkMode) || !new[] { "remote", "online", "hybrid" }.Contains(model.WorkMode.ToLower()))
            {
                return BadRequest("WorkMode must be 'remote', 'online', or 'hybrid'.");
            }

            // Updates job fields with new values
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

            // Handles image update if a new file is provided
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var blobServiceClient = new BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();

                // Deletes the old image if it exists
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

                // Uploads the new image
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
                // Saves updated job to the database
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                // Logs and returns 500 error if update fails
                Console.WriteLine($"Error updating job: {ex.Message}");
                return StatusCode(500, "Failed to update job due to a database error.");
            }
        }

        // DELETE: api/Jobs/{id}
        // Deletes a job and its associated image
        [HttpDelete("{id}")]
        [AllowAnonymous] // Note: Consider adding authorization for production
        public async Task<IActionResult> DeleteJob(int id)
        {
            // Finds the job by ID
            var job = await _context.Jobs.FindAsync(id);
            if (job == null)
            {
                return NotFound();
            }

            // Logs deletion attempt
            Console.WriteLine($"Attempting to delete job with ID: {id}, Title: {job.Title}");

            // Deletes the associated image from Azure Blob Storage
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

            // Removes the job from the database
            _context.Jobs.Remove(job);

            try
            {
                // Saves changes to PostgreSQL
                int affectedRows = await _context.SaveChangesAsync();
                Console.WriteLine($"Database deletion affected {affectedRows} rows");
                return NoContent();
            }
            catch (DbUpdateException ex)
            {
                // Logs and returns 500 error for database issues
                Console.WriteLine($"Database error deleting job: {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, "Failed to delete job due to a database error.");
            }
            catch (Exception ex)
            {
                // Logs and returns 500 error for unexpected issues
                Console.WriteLine($"Unexpected error deleting job: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred while deleting the job.");
            }
        }
    }

    // Model for job upload data, including form fields and file
    public class JobUploadModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IFormFile ImageFile { get; set; }
        public string JobType { get; set; }
        public string ExpiringDate { get; set; }
        public string CreatedBy { get; set; }
        public string WorkMode { get; set; }
        // Optional fields for job details
        public string? KeyResponsibilities { get; set; }
        public string? EducationalBackground { get; set; }
        public string? TechnicalSkills { get; set; }
        public string? Experience { get; set; }
        public string? SoftSkills { get; set; }
    }
}