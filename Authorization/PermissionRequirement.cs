using Microsoft.AspNetCore.Authorization;

namespace AssessmentPlatform.Backend.Authorization
{
    // This class represents the required permission name for an action.
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public string PermissionName { get; }

        public PermissionRequirement(string permissionName)
        {
            PermissionName = permissionName;
        }
    }
}