using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Security.ApiApplication.Configurations
{
    public class JwtValidationOptionFactory: IJwtValidationOption
    {
        private readonly JwtValidationOption settings;

        public string ValidIssuer => settings.ValidIssuer;
        public bool ValidateIssuer => settings.ValidateIssuer;
        public string ValidAudience => settings.ValidAudience;
        public bool ValidateAudience => settings.ValidateAudience;
        public string SecretKey => settings.SecretKey;

        public JwtValidationOptionFactory(IOptions<JwtValidationOption> options)
        {
            settings = options.Value;
        }

        public TokenValidationParameters CreateTokenValidationParameters()
        {
            var result = new TokenValidationParameters
            {
                ValidateIssuer = ValidateIssuer,
                ValidIssuer = ValidIssuer,

                ValidateAudience = ValidateAudience,
                ValidAudience = ValidAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)),

                RequireExpirationTime = true,
                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };

            return result;
        }
    }
}
