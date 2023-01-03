using System;

namespace models
{
    public class AuthorizedUser
    {
        public string UserId { get; set; }
        public string Role { get; set; }
        public string Username { get; set; }
        public DateTime TokenExpiry { get; set; }
    }
}
