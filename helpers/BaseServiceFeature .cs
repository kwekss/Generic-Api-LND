using Microsoft.AspNetCore.Http;
using models;

namespace helpers
{
    public abstract class BaseServiceFeature
    {

        public HttpContext Context { get; private set; }
        public string Service { get; private set; }
        public string FeatureName { get; private set; }
        public ServiceEndpoint ServiceEndpoint { get; private set; }

    }
}
