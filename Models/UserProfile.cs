using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AssessmentPlatform.Backend.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }

        public int UserIdInt { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        public string ProfilePicture { get; set; } = string.Empty;

        public UserEducation Education { get; set; } = new UserEducation();
        public UserExperience WorkExperience { get; set; } = new UserExperience();
        public UserSkills Skills { get; set; } = new UserSkills();

        public string Name { get; set; } = string.Empty;
        public int? Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string LinkedIn { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }

    public class UserEducation
    {
        public string Text { get; set; } = string.Empty;
        public string[] Evidence { get; set; } = new string[0]; // Array for PostgreSQL
        public string Status { get; set; } = "pending"; // pending, approved, declined
    }

    public class UserExperience
    {
        public string Text { get; set; } = string.Empty;
        public string[] Evidence { get; set; } = new string[0]; // Array for PostgreSQL
        public string Status { get; set; } = "pending"; // pending, approved, declined
    }

    public class UserSkills
    {
        public string Text { get; set; } = string.Empty;
        public string[] Evidence { get; set; } = new string[0]; // Array for PostgreSQL
        public string Status { get; set; } = "pending"; // pending, approved, declined
    }
}