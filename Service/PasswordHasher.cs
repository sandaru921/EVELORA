using BCryptLib = BCrypt.Net.BCrypt;

namespace AssessmentPlatform.Backend.Service;

public static class PasswordHasher
{
    public static string Hash(string password) =>
        BCryptLib.HashPassword(password);

    public static bool Verify(string password, string hash) =>
        BCryptLib.Verify(password, hash);
}