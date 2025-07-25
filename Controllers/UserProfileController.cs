using Microsoft.AspNetCore.Mvc;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public async Task<ActionResult<UserProfile>> GetUserProfile()
        {
            var userIdStr = User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userIdStr || u.Email == userIdStr);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => EF.Property<int>(p, "UserIdInt") == user.Id);
            if (profile == null)
            {
                profile = new UserProfile
                {
                    UserId = userIdStr,
                    ProfilePicture = "https://via.placeholder.com/150.png?text=User"
                };
                _context.Entry(profile).Property("UserIdInt").CurrentValue = user.Id;
                _context.UserProfiles.Add(profile);
                await _context.SaveChangesAsync();
            }

            return Ok(profile);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateUserProfile([FromBody] UserProfile updated)
        {
            if (updated == null) return BadRequest("Updated profile data is required.");

            var userIdStr = User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userIdStr || u.Email == userIdStr);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => EF.Property<int>(p, "UserIdInt") == user.Id);
            if (profile == null) return NotFound();

            profile.Name = updated.Name ?? profile.Name;
            profile.Age = updated.Age ?? profile.Age;
            profile.Gender = updated.Gender ?? profile.Gender;
            profile.Title = updated.Title ?? profile.Title;
            profile.LinkedIn = updated.LinkedIn ?? profile.LinkedIn;
            profile.ProfilePicture = updated.ProfilePicture ?? profile.ProfilePicture;

            if (updated.Education != null)
            {
                profile.Education ??= new UserEducation();
                profile.Education.Text = updated.Education.Text ?? profile.Education.Text;
            }
            if (updated.WorkExperience != null)
            {
                profile.WorkExperience ??= new UserExperience();
                profile.WorkExperience.Text = updated.WorkExperience.Text ?? profile.WorkExperience.Text;
            }
            if (updated.Skills != null)
            {
                profile.Skills ??= new UserSkills();
                profile.Skills.Text = updated.Skills.Text ?? profile.Skills.Text;
            }

            _context.Entry(profile).Property("UserIdInt").CurrentValue = user.Id;
            await _context.SaveChangesAsync();

            return Ok(profile);
        }

        [HttpPost("evidence/upload")]
        public async Task<IActionResult> UploadEvidence([FromServices] IOptions<AzureStorageConfig> azureConfig)
        {
            if (azureConfig == null || string.IsNullOrEmpty(azureConfig.Value.ConnectionString)) 
                return StatusCode(500, "Azure Storage configuration is missing.");

            var userIdStr = User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userIdStr || u.Email == userIdStr);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => EF.Property<int>(p, "UserIdInt") == user.Id);
            if (profile == null) return NotFound();

            var file = Request.Form.Files.GetFile("evidence");
            if (file == null || file.Length == 0) return BadRequest("No file uploaded.");

            var field = Request.Form["field"].ToString()?.ToLowerInvariant();
            if (string.IsNullOrEmpty(field) || !new[] { "education", "workexperience", "skills" }.Contains(field))
                return BadRequest("Invalid field");

            // Create container if it doesn't exist
            var blobServiceClient = new BlobServiceClient(azureConfig.Value.ConnectionString);
            var containerName = azureConfig.Value.ContainerName?.ToLower() ?? "user-evidence"; // Default to "user-evidence"
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // Upload to Azure Blob Storage
            var safeFileName = Path.GetFileName(file.FileName);
            var fileName = $"{Guid.NewGuid()}_{safeFileName}";
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = GetContentType(fileName) });
            }

            // Update Evidence array safely
            profile.Education ??= new UserEducation { Evidence = new string[0] };
            profile.WorkExperience ??= new UserExperience { Evidence = new string[0] };
            profile.Skills ??= new UserSkills { Evidence = new string[0] };

            switch (field)
            {
                case "education":
                    profile.Education.Evidence = profile.Education.Evidence.Concat(new[] { fileName }).ToArray();
                    break;
                case "workexperience":
                    profile.WorkExperience.Evidence = profile.WorkExperience.Evidence.Concat(new[] { fileName }).ToArray();
                    break;
                case "skills":
                    profile.Skills.Evidence = profile.Skills.Evidence.Concat(new[] { fileName }).ToArray();
                    break;
                default:
                    return BadRequest("Field not processed correctly.");
            }

            await _context.SaveChangesAsync();
            return Ok(new { filename = fileName, url = blobClient.Uri.AbsoluteUri });
        }

        [HttpDelete("evidence/delete")]
[Authorize]
public async Task<IActionResult> DeleteEvidence([FromBody] EvidenceDeleteRequest request, [FromServices] IOptions<AzureStorageConfig> azureEvidenceConfig)
{
    if (azureEvidenceConfig == null || string.IsNullOrEmpty(azureEvidenceConfig.Value?.ConnectionString))
    {
        Console.WriteLine("Azure Evidence Storage configuration is null or invalid.");
        return StatusCode(500, "Azure Evidence Storage configuration is missing or invalid.");
    }

    var userIdStr = User.Identity?.Name;
    if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userIdStr || u.Email == userIdStr);
    if (user == null) return NotFound();

    var profile = await _context.UserProfiles
        .FirstOrDefaultAsync(p => EF.Property<int>(p, "UserIdInt") == user.Id);
    if (profile == null) return NotFound();

    var field = request.Field?.ToLowerInvariant();
    if (string.IsNullOrEmpty(field) || !new[] { "education", "workexperience", "skills" }.Contains(field))
        return BadRequest("Invalid field");

    var fileName = request.FileName;
    if (string.IsNullOrEmpty(fileName)) return BadRequest("File name is required");

    // Delete from Azure Blob Storage
    var blobServiceClient = new BlobServiceClient(azureEvidenceConfig.Value.ConnectionString);
    var containerName = azureEvidenceConfig.Value.ContainerName?.ToLower() ?? "user-evidence";
    var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
    var blobClient = containerClient.GetBlobClient(fileName);

    try
    {
        await blobClient.DeleteIfExistsAsync();
        Console.WriteLine($"Deleted file from Azure: {fileName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting file from Azure: {ex.Message}");
        return StatusCode(500, $"Failed to delete file from storage: {ex.Message}");
    }

    // Update the evidence array in the database
    object? fieldObj = field switch
    {
        "education" => profile.Education,
        "workexperience" => profile.WorkExperience,
        "skills" => profile.Skills,
        _ => null
    };

    if (fieldObj != null)
    {
        switch (fieldObj)
        {
            case UserEducation education:
                education.Evidence = education.Evidence.Where(f => f != fileName).ToArray();
                break;
            case UserExperience experience:
                experience.Evidence = experience.Evidence.Where(f => f != fileName).ToArray();
                break;
            case UserSkills skills:
                skills.Evidence = skills.Evidence.Where(f => f != fileName).ToArray();
                break;
        }
        await _context.SaveChangesAsync();
        Console.WriteLine($"Updated database, removed {fileName} from {field}");
    }

    return Ok(new { message = "Evidence deleted successfully", fileName });
}

public class EvidenceDeleteRequest
{
    public string Field { get; set; }
    public string FileName { get; set; }
}

        [HttpPost("evidence/review")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReviewEvidence([FromBody] EvidenceReviewRequest request)
        {
            if (request == null) return BadRequest("Review request data is required.");

            var userIdStr = User.Identity?.Name;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == userIdStr || u.Email == userIdStr);
            if (currentUser == null || !User.IsInRole("Admin")) return Forbid();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.UserId);
            if (user == null) return NotFound();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => EF.Property<int>(p, "UserIdInt") == user.Id);
            if (profile == null) return NotFound();

            if (string.IsNullOrEmpty(request.Status) || !new[] { "pending", "approved", "declined" }.Contains(request.Status.ToLowerInvariant()))
                return BadRequest("Invalid status");

            switch (request.Field?.ToLowerInvariant())
            {
                case "education":
                    profile.Education ??= new UserEducation();
                    profile.Education.Status = request.Status;
                    break;
                case "workexperience":
                    profile.WorkExperience ??= new UserExperience();
                    profile.WorkExperience.Status = request.Status;
                    break;
                case "skills":
                    profile.Skills ??= new UserSkills();
                    profile.Skills.Status = request.Status;
                    break;
                default:
                    return BadRequest("Invalid field");
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("evidence/download/{filename}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadEvidence(string filename, [FromServices] IOptions<AzureStorageConfig> azureConfig)
        {
            if (azureConfig == null || string.IsNullOrEmpty(azureConfig.Value.ConnectionString)) 
                return StatusCode(500, "Azure Storage configuration is missing.");

            var blobServiceClient = new BlobServiceClient(azureConfig.Value.ConnectionString);
            var containerName = azureConfig.Value.ContainerName?.ToLower() ?? "user-evidence";
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(filename);

            if (!await blobClient.ExistsAsync())
            {
                return NotFound("File not found.");
            }

            var blobDownloadInfo = await blobClient.DownloadAsync();
            return File(blobDownloadInfo.Value.Content, GetContentType(filename), filename);
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers()
        {
            var profiles = await _context.UserProfiles
                .Select(p => new
                {
                    id = p.Id,
                    username = p.UserId,
                    profile = new
                    {
                        profilePicture = p.ProfilePicture ?? "https://via.placeholder.com/50.png?text=User",
                        education = p.Education != null ? new { text = p.Education.Text ?? "", evidence = p.Education.Evidence ?? new string[0], status = p.Education.Status ?? "pending" } : null,
                        workExperience = p.WorkExperience != null ? new { text = p.WorkExperience.Text ?? "", evidence = p.WorkExperience.Evidence ?? new string[0], status = p.WorkExperience.Status ?? "pending" } : null,
                        skills = p.Skills != null ? new { text = p.Skills.Text ?? "", evidence = p.Skills.Evidence ?? new string[0], status = p.Skills.Status ?? "pending" } : null
                    }
                })
                .ToListAsync();

            return Ok(profiles);
        }

        private string GetContentType(string filename)
        {
            var ext = Path.GetExtension(filename).ToLowerInvariant();
            return ext switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream",
            };
        }
    }
}

public class AzureStorageConfig
{
    public string ? ConnectionString { get; set; }
    public string ? ContainerName { get; set; }
}

public class EvidenceReviewRequest
{
    public int UserId { get; set; }
    public string Field { get; set; }
    public string Status { get; set; } // pending, approved, declined
}