using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Secutity.IssuerJWT.Configurations
{
    public class JwtIssuerFactory : IJwtIssuerOptions
    {
        public string Issuer { get; private set; }
        public string Audience { get; private set; }

        public TimeSpan ValidFor { get; private set; }
        public DateTime NotBefore { get; private set; }
        public DateTime IssuedAt { get; private set; }
        public DateTime Expires { get; private set; }

        public SigningCredentials SigningCredentials { get; private set; }

        public JwtIssuerFactory(IOptions<JwtIssuer> options)
        {
            Issuer = options.Value.Issuer;
            Audience = options.Value.Audience;

            IssuedAt = DateTime.UtcNow;
            NotBefore = IssuedAt;

            ValidFor = TimeSpan.FromMinutes(options.Value.ValidFor);
            Expires = IssuedAt.Add(ValidFor);

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(options.Value.SecretKey));
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        }

        public async Task<string> JtiGenerator()
        {
            return await Task.FromResult(Guid.NewGuid().ToString());
        }
    }
}
