namespace TestService.Models
{
    public class UploadedFile
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string FileName { get; set; }
        public string UploadPath { get; set; }
        public string NewFileName { get; set; }
        public string FileUuid { get; set; }
    }
}
