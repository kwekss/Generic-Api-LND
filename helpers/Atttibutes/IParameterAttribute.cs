using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace helpers.Atttibutes
{
    internal interface IParameterAttribute
    {
        void InitAttribute(ParameterInfo instance, HttpContext context, object[] args);
    }
}