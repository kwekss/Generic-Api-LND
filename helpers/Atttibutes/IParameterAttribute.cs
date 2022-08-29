using Microsoft.AspNetCore.Http;
using models;
using System.Reflection;

namespace helpers.Atttibutes
{
    internal interface IParameterAttribute
    {
        void InitAttribute(ParameterInfo instance, HttpContext context, Endpoint endpoint, object[] args);
    }
}