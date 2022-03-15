using helpers.Notifications;
using Microsoft.AspNetCore.Http;
using models;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace helpers
{
    public abstract class BaseServiceFeature
    {
        private readonly IHttpContextAccessor _httpContext;
        protected BaseServiceFeature(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }
        public virtual string Service { get; }
        public virtual string FeatureName { get; }
        public virtual async Task<ApiResponse> ExecuteFeature() {
            return new ApiResponse { Success = false, ResponseMessage = $"I am  {FeatureName} from {Service} not implemented" };
        }

        public async Task<T> GetPayloadFromBody<T>()
        {
            var sr = new StreamReader(_httpContext.HttpContext.Request.Body);
            string data = await sr.ReadToEndAsync(); 
            return JsonSerializer.Deserialize<T>(data);
        }
    }
}
