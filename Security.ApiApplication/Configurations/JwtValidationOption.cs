namespace Security.ApiApplication.Configurations
{
    public class JwtValidationOption
    {
        public string ValidIssuer { get; set; }

        public bool ValidateIssuer { get; set; }

        public string ValidAudience { get; set; }

        public bool ValidateAudience { get; set; }

        public string SecretKey { get; set; }
    }
}
