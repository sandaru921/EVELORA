namespace AssessmentPlatform.DTO
{
    public class UserLoginDTO
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string Password { get; set; } = string.Empty;
    }
}
