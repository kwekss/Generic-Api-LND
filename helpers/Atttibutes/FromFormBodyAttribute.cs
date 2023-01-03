using helpers.Interfaces;
using Microsoft.AspNetCore.Http;
using models;
using System;
using System.Linq;
using System.Reflection;

namespace helpers.Atttibutes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class FromFormBodyAttribute : Attribute, IParameterAttribute
    {
        public void InitAttribute(ParameterInfo instance, HttpContext context, Endpoint endpoint, params object[] args)
        {
            args[instance.Position] = GetPayloadFromBody(endpoint, instance);
        }

        public dynamic GetPayloadFromBody(Endpoint endpoint, ParameterInfo instance)
        {
            if (typeof(FileContent).Equals(instance.ParameterType)) return endpoint?.Files.FirstOrDefault(_ => _?.Name.ToLower() == instance.Name);

            return null;
        }
    }
}
