using System.Collections.Generic;
using System.Threading.Tasks;

namespace helpers.Engine
{
    public interface IHttpHelper
    {
        Task<T> Get<T>(string url, dynamic payload, List<(string key, string value)> headers = null);
        Task<T> Post<T>(string url, dynamic payload, List<(string key, string value)> headers = null);
    }
}