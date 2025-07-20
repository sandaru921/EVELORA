using System.Text.Json.Serialization;

namespace AssessmentPlatform.Backend.DTO
{
    public class RankingDTO
    {
        [JsonPropertyName("rank")]
        public int Rank { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("marks")]
        public int Marks { get; set; }
        
        [JsonPropertyName("totalMarks")]
        public int TotalMarks { get; set; }
        
        [JsonPropertyName("percentage")]
        public double Percentage { get; set; }
        
        [JsonPropertyName("category")]
        public string Category { get; set; }
        
        [JsonPropertyName("timeTaken")]
        public int TimeTaken { get; set; }
        
        [JsonPropertyName("jobRole")]
        public string JobRole { get; set; }
        
        [JsonPropertyName("quizName")]
        public string QuizName { get; set; }
    }
} 