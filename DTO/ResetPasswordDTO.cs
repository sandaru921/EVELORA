namespace AssessmentPlatform.Backend.DTO
{
    public class ResetPasswordDto
    {
        public string Email { get; set; } = null!;   // ADD THIS
        // public string Token { get; set; } = null!;   // Optional for now
        public string NewPassword { get; set; } = null!;
    }
}