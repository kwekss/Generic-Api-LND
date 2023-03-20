namespace models
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string ResponseMessage { get; set; }
        public dynamic Data { get; set; }
    }
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string ResponseMessage { get; set; }
        public T Data { get; set; }
    }
}
