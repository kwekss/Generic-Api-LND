using System;

namespace AuthService.Models
{
    public class JwtInfo
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
