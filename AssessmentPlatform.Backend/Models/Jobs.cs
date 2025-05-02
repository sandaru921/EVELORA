public class Jobs
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string JobType { get; set; }  // Full-time, Part-time, etc.
    public string Description { get; set; }
    public string ImageUrl { get; set; } // Store image path
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
