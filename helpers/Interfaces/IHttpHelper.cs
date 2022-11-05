using helpers.Engine;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace helpers.Interfaces
{
    public interface IHttpHelper
    {
        HttpClientBuilder ClientBuilder(string clientName = null);
        Task<T> Get<T>(string url, dynamic payload, List<(string key, string value)> headers = null, bool returnRaw = false);
        Task<T> Post<T>(string url, dynamic payload, List<(string key, string value)> headers = null, bool returnRaw = false);
        Task<T> Post<T>(string url, MultipartFormDataContent payload, List<(string key, string value)> headers = null, bool returnRaw = false);
        Task<T> Post<T>(string url, XmlDocument payload, List<(string key, string value)> headers = null, bool returnRaw = false);
    }
}