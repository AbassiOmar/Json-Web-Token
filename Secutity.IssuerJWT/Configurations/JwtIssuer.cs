namespace Secutity.IssuerJWT.Configurations
{
    public class JwtIssuer
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string SecretKey { get; set; }
        public int ValidFor { get; set; }
    }
}
