namespace AssessmentPlatform.DTO
{
    public class GoogleRegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Credential { get; set; } // Google ID token
    }
}