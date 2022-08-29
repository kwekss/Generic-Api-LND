using Microsoft.AspNetCore.Http;
using models;
using System;

namespace helpers.Atttibutes
{
    internal interface IEntryAttribute
    {
        void InitAttribute(HttpContext context, Endpoint endpoint, IServiceProvider serviceProvider);
    }
}