using Microsoft.AspNetCore.Http;
using models;

namespace helpers.Interfaces
{
    public interface IFeatureContext
    {
        Endpoint Endpoint { get; }
        HttpContext HttpContext { get; }
    }
}