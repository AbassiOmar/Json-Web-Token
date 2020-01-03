using Microsoft.IdentityModel.Tokens;

namespace Security.ApiApplication.Configurations
{
    public interface IJwtValidationOption
    {
        string ValidIssuer { get; }
        bool ValidateIssuer { get; }

        string ValidAudience { get; }
        bool ValidateAudience { get; }

        string SecretKey { get; }

        TokenValidationParameters CreateTokenValidationParameters();
    }
}
