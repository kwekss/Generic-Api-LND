using helpers.Notifications;
using Microsoft.AspNetCore.Http;
using models;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Text;

namespace helpers.Atttibutes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromJsonBodyAttribute : Attribute, IParameterAttribute
    {
        public void InitAttribute(ParameterInfo instance, HttpContext context, Endpoint endpoint, params object[] args)
        {
            args[instance.Position] = GetPayloadFromBody(endpoint, instance.ParameterType);
        }

        public dynamic GetPayloadFromBody(Endpoint endpoint, Type type)
        {
            var payloadString = Encoding.UTF8.GetString(endpoint.RequestBody);
            Event.Dispatch("log", $"Request Payload: {payloadString}");
            return JsonConvert.DeserializeObject(payloadString, type);
        }
    }
}
