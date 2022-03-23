using Microsoft.AspNetCore.Http;
using models;

namespace helpers.Engine
{
    public interface IFeatureContext
    {
        Endpoint Endpoint { get; }
        HttpContext HttpContext { get; }
    }
}