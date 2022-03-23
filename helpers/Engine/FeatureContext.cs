using Microsoft.AspNetCore.Http;
using models;
using System.Linq;

namespace helpers.Engine
{
    public class FeatureContext : IFeatureContext
    {
        public FeatureContext(IHttpContextAccessor httpContext)
        { 
            var path = httpContext.HttpContext.Request.Path.Value.Split('/').Where(_ => !string.IsNullOrWhiteSpace(_)).ToList();
            if (path.Count < 2)
            {
                Endpoint = new Endpoint(path);
            }
        }

        public HttpContext HttpContext { get; }
        public Endpoint Endpoint { get; }
    }
}
