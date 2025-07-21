using BCryptLib = BCrypt.Net.BCrypt;

namespace AssessmentPlatform.Backend.Services;

public static class PasswordHasher
{
    //Convert plain password to secure hashed password
    public static string Hash(string password) =>
        BCryptLib.HashPassword(password);

    //Check if given password matches the stored hash
    public static bool Verify(string password, string hash) =>
        BCryptLib.Verify(password, hash);
}