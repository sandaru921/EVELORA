using System;
using System.ComponentModel.DataAnnotations;

namespace AssessmentPlatform.Backend.Models
{
    // Represents a chat message entity in the database
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Message text is required.")]
        [StringLength(500, ErrorMessage = "Message text cannot exceed 500 characters.")]
        public string? Text { get; set; }

        [Required(ErrorMessage = "Sender is required.")]
        [StringLength(50, ErrorMessage = "Sender cannot exceed 50 characters.")]
        public string? Sender { get; set; }

        [Required(ErrorMessage = "Recipient is required.")]
        [StringLength(50, ErrorMessage = "Recipient cannot exceed 50 characters.")]
        public string? Recipient { get; set; }

        public DateTimeOffset Timestamp { get; set; } //= DateTimeOffset.UtcNow;

        [Required(ErrorMessage = "Role is required.")]
        [StringLength(10, ErrorMessage = "Role cannot exceed 10 characters.")]
        public string? Role { get; set; } // "User" or "Admin"
    }
}