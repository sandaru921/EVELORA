namespace AssessmentPlatform.Backend.DTO
{
    public class UserWithPermissionsDTO
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public List<PermissionDTO> Permissions { get; set; } = new();
    }
}