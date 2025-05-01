
namespace AssessmentPlatform.Models{
public class Jobs
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string JobType { get; set; }  // Full-time, Part-time, etc.
    public required string Description { get; set; }
    public required string ImageUrl { get; set; } // Store image path
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
}