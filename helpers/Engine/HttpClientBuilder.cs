﻿using Newtonsoft.Json;
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
        private bool _loggingEnabled { get; set; }
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
            _loggingEnabled = true;
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

        public HttpClientBuilder Timeout(TimeSpan timeSpan)
        { 
            _client.Timeout = timeSpan;
            return this;
        }

        public HttpClientBuilder AddPayload(string payload, string contentType = "application/json")
        {
            _payload = new StringContent(payload, Encoding.UTF8, contentType);
            if (_loggingEnabled) Log.Information($"HTTP Request Payload [{_id}]: {payload.Stringify()}");
            return this;
        }

        public HttpClientBuilder AddPayload(object payload)
        {
            _payload = new StringContent(payload.Stringify(), Encoding.UTF8, "application/json");
            if (_loggingEnabled) Log.Information($"HTTP Request Payload [{_id}]: {payload.Stringify()}");
            return this;
        }

        public HttpClientBuilder AddQueryParams(object payload)
        {
            var payloadObject = (JObject)JsonConvert.DeserializeObject(payload.Stringify());
            List<JProperty> payloadObjectList = payloadObject.Children().Cast<JProperty>().ToList();
            string requestPayload = string.Join("&", payloadObjectList.Select(jp => jp.Name + "=" + HttpUtility.UrlEncode(jp.Value.ToString())));
            if (_loggingEnabled) Log.Information($"HTTP Request Query: {requestPayload}");

            _url = $"{_url}?{requestPayload}";

            return this;
        }
        public HttpClientBuilder AddPayload(XmlDocument payload, string contentType = "text/xml")
        {
            var xml_string_payload = payload.OuterXml;
            xml_string_payload = Regex.Replace(xml_string_payload, ">\\s+", ">");
            xml_string_payload = Regex.Replace(xml_string_payload, "\\s+<", "<");

            if (_loggingEnabled) Log.Information($"HTTP Request Payload [{_id}]: {xml_string_payload}");

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
        public HttpClientBuilder EnableLogging(bool state)
        {
            _loggingEnabled = state;
            return this;
        }
        public HttpClientBuilder RetryIf(Func<HttpClientBuilder, bool> condition)
        {
            _retryCondition = condition;
            return this;
        }

        public async Task<HttpClientBuilder> Execute(bool logPayload = true)
        {
            if (_retries == 0 && _headers != null && _headers.Any())
            {
                for (int i = 0; i < _headers.Count; i++)
                {
                    var isAdded = _client.DefaultRequestHeaders.TryAddWithoutValidation(_headers[i].key, _headers[i].value);
                    if (!isAdded && _loggingEnabled) Log.Warning($"Failed to add header => {_headers[i].key} = {_headers[i].value}");
                }
            }

            await ExecuteRequest(logPayload);

            if (_retryMax > 1 && _retries < _retryMax && _retryCondition(this))
            {
                if (_loggingEnabled) Log.Information($"Retrying HTTP Request with ID: {_id}");

                _retries++;
                await Execute();
            }
            return this;
        }


        private async Task<HttpClientBuilder> ExecuteRequest(bool logPayload = true)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            HttpResponseMessage response = null;
            if (_loggingEnabled)
            {
                Log.Information($"HTTP Request Path [{_id}]: {_url}");
                Log.Information($"HTTP Request Headers [{_id}]: {_client.DefaultRequestHeaders.Stringify()}");
                Log.Information($"HTTP Response Start Time [{_id}]: {DateTime.Now}");
            }

            if (_method.ToLower() == "post") response = await _client.PostAsync(_url, _payload);
            if (_method.ToLower() == "put") response = await _client.PutAsync(_url, _payload);
            if (_method.ToLower() == "delete") response = await _client.DeleteAsync(_url);
            if (_method.ToLower() == "patch") response = await _client.PatchAsync(_url, _payload);
            if (_method.ToLower() == "get" || response == null) response = await _client.GetAsync(_url);

            getResponseCookies(response);
            if (_loggingEnabled) Log.Information($"HTTP Response Headers [{_id}]: {response.Headers.Stringify()}");

            _statusCode = (int)response.StatusCode;
            _responsePayload = await response.Content.ReadAsStringAsync();

            watch.Stop();
            if (_loggingEnabled)Log.Information($"HTTP Response End Time [{_id}]: {DateTime.Now}, Duration: {watch.ElapsedMilliseconds} ms");
            if (_loggingEnabled) Log.Information($"HTTP Response Payload [{_id}]: RC: {_statusCode}, {_responsePayload}");
                 
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

        public int StatusCode() => _statusCode;
        public override string ToString() => _responsePayload;
        public T ToObject<T>() => _responsePayload.ParseObject<T>();

    }
}
