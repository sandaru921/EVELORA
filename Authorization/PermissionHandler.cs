using System.Security.Claims;
using AssessmentPlatform.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AssessmentPlatform.Backend.Authorization;
// This class checks if a user has a specific permission.
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly AppDbContext _context;

    public PermissionHandler(AppDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Get the user ID from the JWT token or logged-in user
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return; // No user, stop checking

        var userId = int.Parse(userIdClaim.Value);

        // Check if user has the required permission
        var hasPermission = await _context.UserPermissions
            .Include(up => up.Permission)
            .AnyAsync(up => up.UserId == userId && up.Permission.Name == requirement.PermissionName);

        if (hasPermission)
        {
            context.Succeed(requirement);// Pass authorization
        }
    }
}