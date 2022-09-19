using Microsoft.AspNetCore.Http;
using models;
using System.Reflection;

namespace helpers.Interfaces
{
    internal interface IParameterAttribute
    {
        void InitAttribute(ParameterInfo instance, HttpContext context, Endpoint endpoint, object[] args);
    }
}