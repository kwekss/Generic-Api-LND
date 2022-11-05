using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace helpers.Engine
{
    public class HttpClientBuilder
    {
        private readonly HttpClient _client;
        private Guid _id { get; set; }
        private string _responsePayload { get; set; }
        private int _statusCode{ get; set; }
        private string _url { get; set; }
        private string _method { get; set; }
        private StringContent _payload { get; set; }
        private List<(string key, string value)> _headers { get; set; } = new List<(string key, string value)>();

        public HttpClientBuilder(HttpClient client)
        {
            _client = client;
            _id = Guid.NewGuid();
        }

        public HttpClientBuilder Url(string url, string method = "GET")
        {
            _url = url;
            _method = method;
            return this;
        }

        public HttpClientBuilder AddHeader(string key, string value)
        {
            _headers.Add((key, value));
            return this;
        }

        public HttpClientBuilder AddPayload(string payload, string contentType = "application/json")
        {
            _payload = new StringContent(payload, Encoding.UTF8, contentType);
            return this;
        }
        public HttpClientBuilder AddPayload(XmlDocument payload, string contentType = "text/xml")
        {
            var xml_string_payload = payload.OuterXml;
            xml_string_payload = Regex.Replace(xml_string_payload, ">\\s+", ">");
            xml_string_payload = Regex.Replace(xml_string_payload, "\\s+<", "<");
            Log.Information($"HTTP Request Payload [{_id}]: {xml_string_payload}");
            _payload = new StringContent(xml_string_payload, Encoding.UTF8, contentType);
            return this;
        }
        public async Task<HttpClientBuilder> Execute()
        {
            Log.Information($"HTTP Request Path [{_id}]: {_url}"); 
            Log.Information($"HTTP Request Headers [{_id}]: {_headers.Stringify()}");
            
            if (_headers != null && _headers.Any())
            {
                for (int i = 0; i < _headers.Count; i++)
                {
                    _client.DefaultRequestHeaders.Add(_headers[i].key, _headers[i].value);
                }
            }
            
            HttpResponseMessage response = null;

            if(_method.ToLower() == "post") response = await _client.PostAsync(_url, _payload);
            if(_method.ToLower() == "put") response = await _client.PutAsync(_url, _payload);
            if(_method.ToLower() == "delete") response = await _client.DeleteAsync(_url);
            if(_method.ToLower() == "patch") response = await _client.PatchAsync(_url, _payload);
            if(_method.ToLower() == "get" || response == null) response = await _client.GetAsync(_url);
            
            _statusCode = (int)response.StatusCode;
            _responsePayload = await response.Content.ReadAsStringAsync();
            Log.Information($"Response Payload: {_responsePayload}");

            return this;
        }

        public int StatusCode() => _statusCode;
        public override string ToString() => _responsePayload;
        public T ToObject<T>() => _responsePayload.ParseObject<T>();
         
    }
}
