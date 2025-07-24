// Models/User.cs
using System.ComponentModel.DataAnnotations.Schema;
namespace AssessmentPlatform.Backend.Models
{
    [Table("Users")]
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string HashPassword { get; set; }
        public bool IsGoogleUser { get; set; } = false;
        // Navigation property for the many-to-many relationship
        public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
