using helpers.Notifications;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace helpers.Engine
{
    public class HttpHelper : IHttpHelper
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HttpHelper(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<T> Post<T>(string url, dynamic payload, List<(string key, string value)> headers = null)
        {
            var client = new HttpClient();

            string requestPayload = JsonConvert.SerializeObject(payload);

            Event.Dispatch(
                "log", $"Request Started @ {DateTime.Now:yyyy-MM-dd hh:mm:ss tt}",
                $"Request Path: {url}\nRequest Payload: {requestPayload}",
                $"Request Headers: {JsonConvert.SerializeObject(headers)}"
             );

            if (headers != null && headers.Any())
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    client.DefaultRequestHeaders.Add(headers[i].key, headers[i].value);
                }
            }

            HttpResponseMessage response = await client.PostAsync(url, new StringContent(requestPayload, Encoding.UTF8, "application/json"));
            Event.Dispatch($"Response StatusCode: {response.StatusCode}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string contents = await response.Content.ReadAsStringAsync();
                Event.Dispatch("log", $"Response Payload: {contents}", $"Request End @ {DateTime.Now:yyyy-MM-dd hh:mm:ss tt}");
                return JsonConvert.DeserializeObject<T>(contents);
            }

            Event.Dispatch("log", $"Request End @ {DateTime.Now:yyyy-MM-dd hh:mm:ss tt}");

            return default(T);
        }
        public async Task<T> Get<T>(string url, dynamic payload, List<(string key, string value)> headers = null)
        {
            var client = new HttpClient();

            var payloadObject = (JObject)JsonConvert.DeserializeObject(payload.ToString());
            List<JProperty> payloadObjectList = payloadObject.Children().Cast<JProperty>().ToList();
            string requestPayload = string.Join("&", payloadObjectList.Select(jp => jp.Name + "=" + HttpUtility.UrlEncode(jp.Value.ToString())));

            url = $"{url}{requestPayload}";

            Event.Dispatch("log",
                $"Request Started @ {DateTime.Now:yyyy-MM-dd hh:mm:ss tt}",
                $"Request Path: {url}\nRequest Payload: {requestPayload}",
                $"Request Headers: {JsonConvert.SerializeObject(headers)}"
            );

            if (headers != null && headers.Any())
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    client.DefaultRequestHeaders.Add(headers[i].key, headers[i].value);
                }
            }

            HttpResponseMessage response = await client.GetAsync(url);
            Event.Dispatch("log", $"Response StatusCode: {response.StatusCode}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string contents = await response.Content.ReadAsStringAsync();
                Event.Dispatch("log", $"Response Payload: {contents}\nRequest End @ {DateTime.Now:yyyy-MM-dd hh:mm:ss tt}");
                return JsonConvert.DeserializeObject<T>(contents);
            }

            Event.Dispatch("log", $"Response: {JsonConvert.SerializeObject(response)}");

            return default(T);
        }

    }
}
