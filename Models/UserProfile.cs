using System.ComponentModel.DataAnnotations;

namespace AssessmentPlatform.Backend.Models
{
    public class UserProfile
    {
        // Primary key for the UserProfile table
        [Key]
        public int Id { get; set; }

        // Foreign key to match User.Id (int)
        public int UserIdInt { get; set; } // Added for foreign key relationship

        // Keep this for frontend compatibility (string userId)
        [Required]
        public string UserId { get; set; } = string.Empty;

        // Additional profile fields as shown in the frontend
        public string ProfilePicture { get; set; } = string.Empty; // URL or base64 string
        public string Education { get; set; } = string.Empty; // Multi-line text
        public string WorkExperience { get; set; } = string.Empty; // Multi-line text
        public string Skills { get; set; } = string.Empty; // Multi-line text
        public string Name { get; set; } = string.Empty;
        public int? Age { get; set; } // Nullable integer
        public string Gender { get; set; } = string.Empty;
        public string LinkedIn { get; set; } = string.Empty;
    }
}

