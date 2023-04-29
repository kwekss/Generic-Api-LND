using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace helpers.Engine
{
    public class HttpClientBuilder
    {
        private readonly HttpClient _client;
        private Guid _id { get; set; }
        private string _responsePayload { get; set; }
        private int _statusCode { get; set; }
        private int _retryMax { get; set; } = 1;
        private int _retries { get; set; }
        private string _url { get; set; }
        private string _method { get; set; }
        private Func<HttpClientBuilder, bool> _retryCondition { get; set; }
        private List<Cookie> _cookieEntries { get; set; } = new List<Cookie>();
        private StringContent _payload { get; set; }
        private List<(string key, string value)> _headers { get; set; } = new List<(string key, string value)>();

        public HttpClientBuilder(HttpClient client)
        {
            _client = client;
            _id = Guid.NewGuid();
            _retryCondition = (self) => false;
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
            Log.Information($"HTTP Request Payload [{_id}]: {payload.Stringify()}");
            return this;
        }

        public HttpClientBuilder AddPayload(object payload)
        {
            _payload = new StringContent(payload.Stringify(), Encoding.UTF8, "application/json");
            Log.Information($"HTTP Request Payload [{_id}]: {payload.Stringify()}");
            return this;
        }

        public HttpClientBuilder AddQueryParams(object payload)
        {
            var payloadObject = (JObject)JsonConvert.DeserializeObject(payload.ToString());
            List<JProperty> payloadObjectList = payloadObject.Children().Cast<JProperty>().ToList();
            string requestPayload = string.Join("&", payloadObjectList.Select(jp => jp.Name + "=" + HttpUtility.UrlEncode(jp.Value.ToString())));
            Log.Information($"Request Query: {requestPayload}");

            _url = $"{_url}?{requestPayload}";

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
        public HttpClientBuilder AddCookie(Cookie cookie)
        {
            _cookieEntries.Add(cookie);
            return this;
        }
        public HttpClientBuilder RetryMax(int retry)
        {
            _retryMax = retry;
            return this;
        }
        public HttpClientBuilder RetryIf(Func<HttpClientBuilder, bool> condition)
        {
            _retryCondition = condition;
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

            /*if (_cookieEntries != null && _cookieEntries.Any())
            {
                _client.DefaultRequestHeaders.Add("Cookie", ToHeaderFormat(_cookieEntries));
            }*/

            HttpResponseMessage response = null;

            Log.Information($"Response Start Time [{_id}]: {DateTime.Now}");
            if (_method.ToLower() == "post") response = await _client.PostAsync(_url, _payload);
            if (_method.ToLower() == "put") response = await _client.PutAsync(_url, _payload);
            if (_method.ToLower() == "delete") response = await _client.DeleteAsync(_url);
            if (_method.ToLower() == "patch") response = await _client.PatchAsync(_url, _payload);
            if (_method.ToLower() == "get" || response == null) response = await _client.GetAsync(_url);

            getResponseCookies(response);
            _statusCode = (int)response.StatusCode;
            _responsePayload = await response.Content.ReadAsStringAsync();

            Log.Information($"Response End Time [{_id}]: {DateTime.Now}");
            Log.Information($"Response Status [{_id}]: {_statusCode}");
            Log.Information($"Response Payload [{_id}]: {_responsePayload}");

            if (_retryMax > 1 && _retries < _retryMax && _retryCondition(this))
            {
                Log.Information($"Retrying HTTP Request with ID: {_id}");
                _retries++;
                await Execute();
            }
            return this;
        }


        private void getResponseCookies(HttpResponseMessage response)
        {
            if (!response.Headers.TryGetValues("Set-Cookie", out var cookieEntries))
            {
                return;
            }

            var uri = response.RequestMessage.RequestUri;
            var cookieContainer = new CookieContainer();

            foreach (var cookieEntry in cookieEntries)
            {
                cookieContainer.SetCookies(uri, cookieEntry);
            }

            _cookieEntries = new List<Cookie> { };
            _cookieEntries.AddRange(cookieContainer.GetCookies(uri).Cast<Cookie>());
        }
        public List<Cookie> GetCookies()
        {
            return _cookieEntries;
        }

        private string ToHeaderFormat(IEnumerable<Cookie> cookies)
        {
            var cookieString = string.Empty;
            var delimiter = string.Empty;

            foreach (var cookie in cookies)
            {
                cookieString += delimiter + cookie;
                delimiter = "; ";
            }

            return cookieString;
        }

        public int StatusCode() => _statusCode;
        public override string ToString() => _responsePayload;
        public T ToObject<T>() => _responsePayload.ParseObject<T>();

    }
}
