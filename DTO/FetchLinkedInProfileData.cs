using System.Text.Json.Serialization;
namespace AssessmentPlatform.Backend.DTO

{
    public class LinkedInFetchRequest
    {
        public string LinkedIn { get; set; } = string.Empty;

        public string AccessToken { get; set; }
    }
}
