using Microsoft.AspNetCore.Http;
using models;

namespace helpers.Interfaces
{
    public interface IFeatureContext
    {
        ServiceEndpoint Endpoint { get; }
        HttpContext HttpContext { get; }
    }
}