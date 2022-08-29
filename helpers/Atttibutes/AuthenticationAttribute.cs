using helpers.Engine;
using helpers.Exceptions;
using helpers.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace helpers.Atttibutes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class AuthenticationAttribute : Attribute, IEntryAttribute
    {
        public enum AuthenticationType
        {
            Integrator
        }
        public AuthenticationType Schema { get; set; }
        public string RequestTimeKey { get; set; }
        public AuthenticationAttribute()
        {

        }

        public void InitAttribute(HttpContext context, Endpoint endpoint, IServiceProvider services)
        {
            if (Schema == AuthenticationType.Integrator)
                ValidateIntegrator(context, services, endpoint);
        }

        private void ValidateIntegrator(HttpContext context, IServiceProvider services, Endpoint endpoint)
        {
            var authorizationString = context.Request.Headers["Authorization"].ToString();
            var payloadObj = GetPayloadFromBody(endpoint).Result;
            if (payloadObj == null) throw new CustomException($"Invalid request payload");
            var payload = JObject.FromObject(payloadObj);

            var pathValue = payload.SelectToken(string.IsNullOrWhiteSpace(RequestTimeKey) ? "RequestTime" : RequestTimeKey);

            if (string.IsNullOrWhiteSpace(pathValue?.ToString())) throw new CustomException($"Request timestamp must be set");

            DateTime requestTime = Convert.ToDateTime(pathValue.ToString());

            var integrator = services.GetService<IIntegratorHelper>();
            Event.Dispatch("log", $"Auth String: {authorizationString}");
            if (integrator != null)
            {
                var validateRequest = integrator.ValidateIntegrator(authorizationString, requestTime).Result;
                if (!string.IsNullOrWhiteSpace(validateRequest))
                {
                    Event.Dispatch("log", $"Validation: {validateRequest}");
                    throw new CustomException($"{validateRequest}");
                }
            }
        }

        private async Task<dynamic> GetPayloadFromBody(Endpoint endpoint)
        {
            var data = Encoding.UTF8.GetString(endpoint.RequestBody);
            return JsonConvert.DeserializeObject(data);
        }
    }
}
