using Microsoft.AspNetCore.Http;
using models;
using System;

namespace helpers.Interfaces
{
    internal interface IEntryAttribute
    {
        void InitAttribute(HttpContext context, ServiceEndpoint endpoint, IServiceProvider serviceProvider);
    }
}