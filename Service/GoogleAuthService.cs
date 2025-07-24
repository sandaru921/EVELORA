using Google.Apis.Auth;

namespace AssessmentPlatform.Backend.Service
{
    public class GoogleAuthService
    {
        private readonly IConfiguration _configuration;

        public GoogleAuthService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<GoogleJsonWebSignature.Payload?> VerifyGoogleTokenAsync(string credential)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { _configuration["Authentication:GoogleClientId"] } // from appsettings.json
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);
                return payload;
            }
            catch
            {
                return null;
            }
        }
    }
}