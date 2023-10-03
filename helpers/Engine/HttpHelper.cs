using helpers.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace helpers.Engine
{
    public class HttpHelper : IHttpHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;
 
        public HttpHelper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory; 
        }

        public HttpClientBuilder ClientBuilder(string clientName = null)
        {
            var client = string.IsNullOrWhiteSpace(clientName) ? _httpClientFactory.CreateClient() : _httpClientFactory.CreateClient(clientName);
            return new HttpClientBuilder(client);
        }
         
    }
}
