// Models/User.cs
namespace AssessmentPlatform.Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string HashPassword { get; set; }
        // public string Auth0Id { get; set; }
        
        // Navigation property for the many-to-many relationship
        public ICollection<UserPermission> UserPermissions { get; set; }
    }
}
