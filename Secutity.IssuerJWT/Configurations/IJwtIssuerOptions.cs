using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Secutity.IssuerJWT.Configurations
{
   public  interface IJwtIssuerOptions
    {
        String Issuer { get; }
        String Audience { get; }
        TimeSpan ValidFor { get; }
        DateTime NotBefore { get; }
        DateTime IssuedAt { get; }
        DateTime Expires { get; }

        Task<String> JtiGenerator();

        SigningCredentials SigningCredentials { get; }
    }
}
