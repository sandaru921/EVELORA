
namespace AssessmentPlatform.Backend.DTO
{
public class JobQuizDetailsUpdateDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public IFormFile ImageFile { get; set; }
    public string JobType { get; set; }
    public string ExpiringDate { get; set; }
    public string CreatedBy { get; set; }
    public string WorkMode { get; set; }
    public List<string> KeyResponsibilities { get; set; }
    public List<string> EducationalBackground { get; set; }
    public List<string> TechnicalSkills { get; set; }
    public List<string> Experience { get; set; }
    public List<string> SoftSkills { get; set; }
    public int? QuizId { get; set; }
}
}