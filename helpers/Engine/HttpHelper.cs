using helpers.Interfaces;
using helpers.Notifications;
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
        private readonly IMessengerHub _messengerHub;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerSettings _serializerSettings;

        public HttpHelper(IMessengerHub messengerHub, IHttpClientFactory httpClientFactory)
        {
            _messengerHub = messengerHub;
            _httpClientFactory = httpClientFactory;
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        public async Task<T> Post<T>(string url, object payload, List<(string key, string value)> headers = null, bool returnRaw = false)
        {

            var client = _httpClientFactory.CreateClient();

            string requestPayload = payload.Stringify();
             
            Log.Information($"HTTP Request Path: {url}");
            Log.Information($"HTTP Request Payload: {requestPayload}");
            Log.Information($"HTTP Request Headers: {headers.Stringify(_serializerSettings)}");
             

            if (headers != null && headers.Any())
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    client.DefaultRequestHeaders.Add(headers[i].key, headers[i].value);
                }
            }

            HttpResponseMessage response = await client.PostAsync(url, new StringContent(requestPayload, Encoding.UTF8, "application/json"));
            Log.Information($"HTTP Response StatusCode: {response.StatusCode}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string contents = await response.Content.ReadAsStringAsync();
                Log.Information($"HTTP Response Payload: {contents}");
                 
                if (returnRaw) return contents as dynamic;

                return contents.ParseObject<T>();
            }

            Log.Information($"HTTP Request End"); 

            return default(T);
        }

        public async Task<T> Post<T>(string url, MultipartFormDataContent payload, List<(string key, string value)> headers = null, bool returnRaw = false)
        {

            HttpClient client = _httpClientFactory.CreateClient();
             
            Log.Information($"HTTP Request Path: {url}");
            Log.Information($"HTTP Request Payload Lenght: {payload.Count()}");
            Log.Information($"HTTP Request Headers: {JsonConvert.SerializeObject(headers)}");
             


            if (headers != null && headers.Any())
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    client.DefaultRequestHeaders.Add(headers[i].key, headers[i].value);
                }
            }

            HttpResponseMessage response = await client.PostAsync(new Uri(url), payload);

            Log.Information($"HTTP Response StatusCode: {response.StatusCode}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string contents = await response.Content.ReadAsStringAsync();
                Log.Information($"HTTP Response Payload: {contents}");
                 
                if (returnRaw) return contents as dynamic;

                return contents.ParseObject<T>();
            }

            Log.Information($"HTTP Request End"); 

            return default(T);

        }

        public async Task<T> Post<T>(string url, XmlDocument payload, List<(string key, string value)> headers = null, bool returnRaw = false)
        {

            HttpClient client = _httpClientFactory.CreateClient();
            var xml_string_payload = payload.OuterXml; 
            xml_string_payload = Regex.Replace(xml_string_payload, ">\\s+", ">");
            xml_string_payload = Regex.Replace(xml_string_payload, "\\s+<", "<");
             
            Log.Information($"HTTP Request Path: {url}");
            Log.Information($"HTTP Request Payload: {xml_string_payload}");
            Log.Information($"HTTP Request Headers: {headers.Stringify()}");
             

            if (headers != null && headers.Any())
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    client.DefaultRequestHeaders.Add(headers[i].key, headers[i].value);
                }
            }

            HttpResponseMessage response = await client.PostAsync(new Uri(url), new StringContent(xml_string_payload, Encoding.UTF8, "text/xml"));

            Log.Information($"Response StatusCode: {response.StatusCode}");
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string contents = await response.Content.ReadAsStringAsync();
                Log.Information($"Response Payload: {contents}"); 

                if (returnRaw) return contents as dynamic;

                return contents.ParseObject<T>();
            }

            Log.Information($"Request End"); 

            return default(T);

        }

        public async Task<T> Get<T>(string url, object payload = null, List<(string key, string value)> headers = null, bool returnRaw = false)
        {
            var client = _httpClientFactory.CreateClient();

            Log.Information($"HTTP Request to {url}");
            Log.Information($"Request Headers: {headers.Stringify()}");

            if (payload != null)
            {
                var payloadObject = (JObject)JsonConvert.DeserializeObject(payload.ToString());
                List<JProperty> payloadObjectList = payloadObject.Children().Cast<JProperty>().ToList();
                string requestPayload = string.Join("&", payloadObjectList.Select(jp => jp.Name + "=" + HttpUtility.UrlEncode(jp.Value.ToString())));
                Log.Information($"Request Query: {requestPayload}");
                url = $"{url}{requestPayload}";
            }


            if (headers != null && headers.Any())
            {
                for (int i = 0; i < headers.Count; i++)
                {
                    client.DefaultRequestHeaders.Add(headers[i].key, headers[i].value);
                }
            }

            HttpResponseMessage response = await client.GetAsync(url);
            Log.Information($"Response StatusCode: {response.StatusCode}");

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string contents = await response.Content.ReadAsStringAsync();
                Log.Information($"Response Payload: {contents}");
                 
                if (returnRaw) return contents as dynamic;

                return contents.ParseObject<T>();
            }

            return default(T);
        }

    }
}
