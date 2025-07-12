
namespace AssessmentPlatform.Backend.DTO
{
    public class AssignPermissionDTO
    {
        public int UserId { get; set; }
        public List<int> PermissionIds { get; set; }
    }
}