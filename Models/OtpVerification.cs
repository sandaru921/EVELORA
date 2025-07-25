using System;
using System.ComponentModel.DataAnnotations;

namespace AssessmentPlatform.Backend.Models
{
    public class OtpVerification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string OtpCode { get; set; } = null!;

        public DateTime ExpiryTime { get; set; }
    }
}