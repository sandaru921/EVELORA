using System.Text.Json.Serialization;

namespace AssessmentPlatform.Backend.DTO
{
    public class LinkedInAccessTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
