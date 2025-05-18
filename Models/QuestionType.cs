using System.ComponentModel.DataAnnotations;

namespace AssessmentPlatform.Backend.Models
{
    public class QuestionType
    {
        [Key]
        public int TypeId { get; set; }

        public string TypeName { get; set; }
    }
}
