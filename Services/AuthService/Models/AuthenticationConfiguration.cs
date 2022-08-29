namespace AuthService.Models
{
    public class AuthenticationConfiguration
    {
        public string JwtEncryptionKey { get; set; }
        public int SessionTimeout { get; set; }
        public string IntegratorAccessToken { get; set; }
    }
}
