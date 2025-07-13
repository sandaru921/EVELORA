namespace AssessmentPlatform.Backend.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public ICollection<UserPermission> UserPermissions { get; set; }
    }
}