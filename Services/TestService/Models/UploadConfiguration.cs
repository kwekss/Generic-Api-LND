using System.Collections.Generic;

namespace TestService.Models
{
    public class UploadConfiguration
    {
        public List<string> AllowedExtensions { get; set; }
        public double AllowedSizeMb { get; set; }
        public string UploadDir { get; set; }
        public bool Replace { get; set; }
    }
}
