using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace helpers.Atttibutes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromJsonBodyAttribute : Attribute,IParameterAttribute
    {
        public void InitAttribute(ParameterInfo instance,  HttpContext context,  params object[] args)
        {
            args[instance.Position] = GetPayloadFromBody(context, instance.ParameterType).Result;
        }

        public async Task<dynamic> GetPayloadFromBody(HttpContext httpContext, Type type)
        {
            var sr = new StreamReader(httpContext.Request.Body);
            string data = await sr.ReadToEndAsync();
            //if(type.Name.ToLower() == "object") return JsonSerializer.Deserialize<dynamic>(data);
            return JsonSerializer.Deserialize(data, type);
        }
    }
}
