using System;

namespace models
{
    public class AuthorizedUser
    {
        public string UserId { get; set; }
        public dynamic Role { get; set; }
        public string Username { get; set; }
        public DateTime TokenExpiry { get; set; }
        public dynamic AdditionalInfo { get; set; }
    }
}
