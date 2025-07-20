using System.Text.Json.Serialization;

namespace AssessmentPlatform.Backend.Models
{
    public class QuizResults
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int QuizId { get; set; }
    public int Score { get; set; }
    public int TotalMarks { get; set; }
    public DateTime SubmissionTime { get; set; }
    public int TimeTaken { get; set; }
        public int UserIdInt { get; set; }
        
        // Navigation properties
        public User User { get; set; }
        public Quiz Quiz { get; set; }
}

public class Ranking
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
    public string JobRole { get; set; }  // Job role for fair comparison
    
    [JsonPropertyName("quizName")]
    public string QuizName { get; set; } // Quiz name for clarity
    }
}