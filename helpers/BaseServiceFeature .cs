using Microsoft.AspNetCore.Http;

namespace helpers
{
    public abstract class BaseServiceFeature
    {

        public HttpContext Context { get; private set; }
        public string Service { get; private set; }
        public string FeatureName { get; private set; }


    }
}
