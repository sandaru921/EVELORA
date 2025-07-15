using AssessmentPlatform.Backend.Models;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssessmentPlatform.Backend.DTO;

namespace AssessmentPlatform.Backend.Service
{
    public class UserService
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public UserService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        // Register new user after checking for duplicates and hashing password
        public async Task<(User?, string?)> RegisterUserAsync(UserRegisterDTO registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return (null, "Email is already registered.");

            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                return (null, "Username is already taken.");

            // Use BCrypt to hash the password
            string hashedPassword = PasswordHasher.Hash(registerDto.Password);

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                HashPassword = hashedPassword
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return (user, null);
        }

        //Login user and return token and permission list if valid
        public async Task<(User? User, string Token, List<string> Permissions)> AuthenticateUserAsync(UserLoginDTO loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.Password) ||
                (string.IsNullOrWhiteSpace(loginDto.Email) && string.IsNullOrWhiteSpace(loginDto.Username)))
            {
                return (null, string.Empty, new List<string>());
            }

            // Fetch the user using either email or username
            var user = await _context.Users
                .Include(u => u.UserPermissions)
                    .ThenInclude(up => up.Permission)
                .FirstOrDefaultAsync(u =>
                (!string.IsNullOrWhiteSpace(loginDto.Email) && u.Email == loginDto.Email) ||
                (!string.IsNullOrWhiteSpace(loginDto.Username) && u.Username == loginDto.Username));

            if (user == null)
                return (null, string.Empty, new List<string>());

            // Use BCrypt to verify the password
            bool isPasswordValid = PasswordHasher.Verify(loginDto.Password, user.HashPassword);
            if (!isPasswordValid)
                return (null, string.Empty, new List<string>());

            // Generate JWT token
            var token = GenerateJwtToken(user);
            // Extract permission names
            var permissions = user.UserPermissions
                .Select(up => up.Permission.Name)
                .ToList();

            return (user, token, permissions);
        }

        // Change user's password
        public async Task<string?> ResetPasswordAsync(ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return "Email and new password are required.";

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return "User not found.";

            user.HashPassword = PasswordHasher.Hash(dto.NewPassword);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return null;
        }

        // Get list of all users with their permissions
        public async Task<List<UserWithPermissionsDTO>> GetAllUsersWithPermissionsAsync()
        {
            var users = await _context.Users
                .Include(u => u.UserPermissions)
                .ThenInclude(up => up.Permission)
                .ToListAsync();

            return users.Select(u => new UserWithPermissionsDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Permissions = u.UserPermissions
                    .Where(up => up.Permission != null)
                    .Select(up => new PermissionDTO
                    {
                        Id = up.Permission.Id,
                        Name = up.Permission.DisplayName
                    }).ToList()
            }).ToList();
        }

        // Create JWT token with user claims
        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

            // Determine the user's role based on their permissions
            var role = user.UserPermissions.Any(up => up.Permission.Name == "Admin") ? "Admin" : "User";

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpireMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
