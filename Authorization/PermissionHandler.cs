using System.Security.Claims;
using AssessmentPlatform.Backend.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace AssessmentPlatform.Backend.Authorization;
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
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return;

        var userId = int.Parse(userIdClaim.Value);

        var hasPermission = await _context.UserPermissions
            .Include(up => up.Permission)
            .AnyAsync(up => up.UserId == userId && up.Permission.Name == requirement.PermissionName);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}