using Microsoft.AspNetCore.Http;
//using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace helpers.Atttibutes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromQueryAttribute : Attribute, IParameterAttribute
    {

        public void InitAttribute(ParameterInfo instance, HttpContext context, object[] args)
        {
            args[instance.Position] = GetPayloadFromQuery(context, instance.ParameterType).Result;
        }


        private async Task<dynamic> GetPayloadFromQuery(HttpContext httpContext, Type type)
        {
            var source = HttpUtility.ParseQueryString(httpContext.Request.QueryString.ToUriComponent());
            string serialized = JsonSerializer.Serialize(source.AllKeys.ToDictionary(k => k, k =>
            {
                dynamic val = source.GetValues(k);
                if (val.Length > 1) return val;
                return (val as Array).GetValue(0);
            }));

            return JsonSerializer.Deserialize(serialized, type);
        }
    }
}
