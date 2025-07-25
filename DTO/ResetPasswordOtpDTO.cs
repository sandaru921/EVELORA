namespace AssessmentPlatform.Backend.DTO
{
    public class ResetPasswordOtpDto
    {
        public string Email { get; set; } = null!;
        public string Otp { get; set; } = null!;
        public string NewPassword { get; set; } = null!; }
}