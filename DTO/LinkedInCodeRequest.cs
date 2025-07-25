using System.Text.Json.Serialization;
namespace AssessmentPlatform.Backend.DTO
{
    public class LinkedInCodeRequest
    {
        public string Code { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public string CodeVerifier { get; set; } = string.Empty;
    }
}
