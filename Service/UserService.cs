using AssessmentPlatform.Backend.Models;
using AssessmentPlatform.Backend.Data;
using AssessmentPlatform.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssessmentPlatform.Backend.Service; // For PasswordHasher

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

        public async Task<(User?, string?)> RegisterUserAsync(UserRegisterDTO userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return (null, "Email is already registered.");

            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
                return (null, "Username is already taken.");

            // ✅ Use BCrypt to hash the password
            string hashedPassword = PasswordHasher.Hash(userDto.Password);
            
            // // Hash the incoming password
            // string HashedPassword = HashPassword(userDto.Password);

            // Create a new UserRegisterDTO object and map
            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                HashPassword = hashedPassword
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return (user, null);
        }

        public async Task<(User? User, string Token)> AuthenticateUserAsync(UserLoginDTO loginDto)
        {
            // string hashedPassword = HashPassword(loginDto.Password);

            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == loginDto.Email || u.Username == loginDto.Username); 
                    // && u.HashPassword == hashedPassword);

            if (user == null)
                return (null, string.Empty);

            // ✅ Use BCrypt to verify the password
            bool isPasswordValid = PasswordHasher.Verify(loginDto.Password, user.HashPassword);
            if (!isPasswordValid)
                return (null, string.Empty);
            
            var token = GenerateJwtToken(user);
            return (user, token);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
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

        // private string HashPassword(string password)
        // {
        //     using var sha256 = SHA256.Create();
        //     var bytes = Encoding.UTF8.GetBytes(password);
        //     var hashBytes = sha256.ComputeHash(bytes);
        //     return Convert.ToBase64String(hashBytes);
        // }
    }
}
