// Controllers/PermissionController.cs
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.Backend.DTO;
using AssessmentPlatform.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssessmentPlatform.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/permission/assign
        [HttpPost("assign")]
        public async Task<IActionResult> AssignPermissions(AssignPermissionDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Remove existing permissions
            user.UserPermissions.Clear();

            // Add new permissions
            foreach (var permId in dto.PermissionIds)
            {
                if (!await _context.Permissions.AnyAsync(p => p.Id == permId))
                    return NotFound($"Permission with ID {permId} not found.");

                user.UserPermissions.Add(new UserPermission
                {
                    UserId = dto.UserId,
                    PermissionId = permId
                });
            }

            await _context.SaveChangesAsync();
            return Ok("Permissions assigned successfully.");
        }

        // GET: api/permission/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPermissions(int userId)
        {
            var userPermissions = await _context.UserPermissions
                .Where(up => up.UserId == userId)
                .Include(up => up.Permission)
                .Select(up => new
                {
                    up.Permission.Id,
                    up.Permission.Name
                })
                .ToListAsync();

            return Ok(userPermissions);
        }
    }
}
