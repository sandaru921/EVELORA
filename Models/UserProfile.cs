// Namespace for the models
namespace AssessmentPlatform.Models
{
    // Represents extended profile data for the user profile page
    public class UserProfile
    {
        public int Id { get; set; } // Unique identifier for each profile
        public int UserId { get; set; } // Foreign key linking to the User table
        //public User User { get; set; } // Navigation property for the related User
        public string ? Education { get; set; } // User's education details (e.g., "BSc IT")
        public string ? ContactDetails { get; set; } // User's contact info (e.g., phone, address)
        public string ? Skills { get; set; } // User's skills as a comma-separated string (e.g., "React, JavaScript")
        public string ? Bio { get; set; } // User's bio or description
        public int? Age { get; set; } // User's age (nullable if not provided)
        public string ? Location { get; set; } // User's location
        public string ? Goals { get; set; } // User's goals
        public string ? Motivations { get; set; } // User's motivations
        public string ? Concerns { get; set; } // User's concerns
        public int? Marks { get; set; } // User's marks (nullable if not applicable)
        public string ? ProfilePictureUrl { get; set; } // URL to the user's profile picture
    }
}