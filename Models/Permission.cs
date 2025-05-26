using System.Collections.Generic;
using AssessmentPlatform.Backend.Models;

namespace AssessmentPlatform.Backend.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public ICollection<UserPermission> UserPermissions { get; set; }
    }
}