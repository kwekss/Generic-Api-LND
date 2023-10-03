using helpers.Engine;

namespace helpers.Interfaces
{
    public interface IHttpHelper
    {
        HttpClientBuilder ClientBuilder(string clientName = null);
    }
}