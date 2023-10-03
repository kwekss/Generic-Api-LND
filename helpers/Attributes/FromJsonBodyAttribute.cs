using helpers.Exceptions;
using helpers.Interfaces;
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
        public void InitAttribute(ParameterInfo instance, HttpContext context, ServiceEndpoint endpoint, params object[] args)
        {
            args[instance.Position] = GetPayloadFromBody(endpoint, instance.ParameterType);
        }

        public dynamic GetPayloadFromBody(ServiceEndpoint endpoint, Type type)
        {
            var payloadString = Encoding.UTF8.GetString(endpoint.RequestBody);
            if (string.IsNullOrWhiteSpace(payloadString)) throw new CustomException("Invalid request payload");
            return JsonConvert.DeserializeObject(payloadString, type);
        }
    }
}
